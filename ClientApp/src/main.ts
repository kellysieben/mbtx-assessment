import { Component, OnInit, OnDestroy, signal } from '@angular/core';
import { bootstrapApplication } from '@angular/platform-browser';
import { CommonModule } from '@angular/common';
import * as signalR from '@microsoft/signalr';

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
  imports: [CommonModule],
  template: `
    <div class="container">
      <h1>Sensor Dashboard</h1>

      <!-- Status Indicator -->
      <div class="status" [class.live]="isLive()" [class.offline]="!isLive()">
        {{ isLive() ? 'LIVE' : 'OFFLINE' }}
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
              <td>{{ r.timestamp | date:'yyyy-MM-dd @ HH:mm:ss' }}</td>
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
export class AppComponent implements OnInit, OnDestroy {
  isLive = signal(false);
  readings = signal<SensorReading[]>([]);

  private hub: signalR.HubConnection | null = null;

  ngOnInit(): void {
    this.connectHub();
  }

  private connectHub(): void {
    this.hub = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/sensor')
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
