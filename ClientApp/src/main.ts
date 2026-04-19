import { Component, OnDestroy, signal } from '@angular/core';
import { bootstrapApplication } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import * as signalR from '@microsoft/signalr';
import { inject } from '@angular/core';

interface SensorReading {
  id: string;
  sequenceNumber: number;
  timestamp: string;
  temperature: number;
  humidity: number;
  co2Ppm: number;
}

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule, HttpClientModule],
  template: `
    <div class="container">
      <h1>Sensor Dashboard</h1>

      <!-- Status Indicator -->
      <div class="status" [class.live]="isLive()" [class.offline]="!isLive()">
        {{ isLive() ? 'LIVE' : 'OFFLINE' }}
      </div>

      <!-- Register -->
      <div class="register-panel">
        <h2>Register Client</h2>
        <input
          [(ngModel)]="clientId"
          placeholder="Enter client ID"
          [disabled]="registered()"
        />
        <button (click)="register()" [disabled]="!clientId || registered()">
          Register
        </button>
        <span *ngIf="registerMsg()" class="msg">{{ registerMsg() }}</span>
      </div>

      <!-- Messages -->
      <div class="readings-panel">
        <h2>Sensor Readings</h2>
        <p *ngIf="readings().length === 0" class="empty">No readings received yet.</p>
        <table *ngIf="readings().length > 0">
          <thead>
            <tr>
              <th>Timestamp</th>
              <th>Seq #</th>
              <th>Temp (°C)</th>
              <th>Humidity (%)</th>
              <th>CO₂ (ppm)</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let r of readings()">
              <td>{{ r.timestamp | date:'HH:mm:ss' }}</td>
              <td>{{ r.sequenceNumber }}</td>
              <td>{{ r.temperature | number:'1.2-2' }}</td>
              <td>{{ r.humidity | number:'1.2-2' }}</td>
              <td>{{ r.co2Ppm }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  `,
})
export class AppComponent implements OnDestroy {
  private http = inject(HttpClient);

  clientId = '';
  registered = signal(false);
  registerMsg = signal('');
  isLive = signal(false);
  readings = signal<SensorReading[]>([]);

  private hub: signalR.HubConnection | null = null;

  register(): void {
    if (!this.clientId) return;
    this.http
      .post(`/clients/register?clientId=${encodeURIComponent(this.clientId)}`, null, { observe: 'response' })
      .subscribe({
        next: () => {
          this.registered.set(true);
          this.registerMsg.set(`Registered as "${this.clientId}".`);
          this.connectHub();
        },
        error: (err) => {
          this.registerMsg.set(err.status === 409
            ? `"${this.clientId}" is already registered.`
            : `Error: ${err.statusText}`);
        },
      });
  }

  private connectHub(): void {
    this.hub = new signalR.HubConnectionBuilder()
      .withUrl(`/hubs/sensor?clientId=${encodeURIComponent(this.clientId)}`)
      .withAutomaticReconnect()
      .build();

    this.hub.on('sensorReadingAvailable', (reading: SensorReading) => {
      this.readings.update((prev) => [reading, ...prev].slice(0, 50));
    });

    this.hub.onclose(() => this.isLive.set(false));
    this.hub.onreconnecting(() => this.isLive.set(false));
    this.hub.onreconnected(() => this.isLive.set(true));

    this.hub.start().then(() => this.isLive.set(true)).catch(() => this.isLive.set(false));
  }

  ngOnDestroy(): void {
    this.hub?.stop();
  }
}

bootstrapApplication(AppComponent).catch(console.error);
