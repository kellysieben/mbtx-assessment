import { Component, OnInit, OnDestroy, signal, computed } from '@angular/core';
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

interface Anomaly {
  id: string;
  detectedAt: string;
  sensorType: string;
  value: number;
  zScore: number;
  reason: string;
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

      <!-- Anomalies -->
      <div class="readings-panel anomalies-panel">
        <h2>Anomalies</h2>
        <p *ngIf="anomalies().length === 0" class="empty">No anomalies detected yet.</p>
        <table *ngIf="anomalies().length > 0">
          <thead>
            <tr>
              <th>Timestamp</th>
              <th>Sensor Type</th>
              <th>Value</th>
              <th>Reason</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let a of anomaliesPage()">
              <td>{{ a.detectedAt | date:'yyyy-MM-dd @ HH:mm:ss' }}</td>
              <td>{{ a.sensorType }}</td>
              <td>{{ a.value | number:'1.2-2' }}</td>
              <td>{{ a.reason }}</td>
            </tr>
          </tbody>
        </table>
        <div class="pagination" *ngIf="anomaliesTotalPages() > 1">
          <button (click)="prevAnomaliesPage()" [disabled]="anomaliesPage$() === 1">&#8249;</button>
          <span>Page {{ anomaliesPage$() }} of {{ anomaliesTotalPages() }}</span>
          <button (click)="nextAnomaliesPage()" [disabled]="anomaliesPage$() === anomaliesTotalPages()">&#8250;</button>
        </div>
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
            <tr *ngFor="let r of readingsPage()">
              <td>{{ r.timestamp | date:'yyyy-MM-dd @ HH:mm:ss' }}</td>
              <td>{{ r.sequenceNumber }}</td>
              <td>{{ r.temperature | number:'1.2-2' }}</td>
              <td>{{ r.humidity | number:'1.2-2' }}</td>
              <td>{{ r.co2Ppm }}</td>
            </tr>
          </tbody>
        </table>
        <div class="pagination" *ngIf="readingsTotalPages() > 1">
          <button (click)="prevReadingsPage()" [disabled]="readingsPage$() === 1">&#8249;</button>
          <span>Page {{ readingsPage$() }} of {{ readingsTotalPages() }}</span>
          <button (click)="nextReadingsPage()" [disabled]="readingsPage$() === readingsTotalPages()">&#8250;</button>
        </div>
      </div>
  `,
})
export class AppComponent implements OnInit, OnDestroy {
  readonly pageSize = 10;

  isLive = signal(false);
  readings = signal<SensorReading[]>([]);
  anomalies = signal<Anomaly[]>([]);

  readingsPage$ = signal(1);
  anomaliesPage$ = signal(1);

  readingsTotalPages = computed(() => Math.max(1, Math.ceil(this.readings().length / this.pageSize)));
  anomaliesTotalPages = computed(() => Math.max(1, Math.ceil(this.anomalies().length / this.pageSize)));

  readingsPage = computed(() => {
    const page = Math.min(this.readingsPage$(), this.readingsTotalPages());
    const start = (page - 1) * this.pageSize;
    return this.readings().slice(start, start + this.pageSize);
  });

  anomaliesPage = computed(() => {
    const page = Math.min(this.anomaliesPage$(), this.anomaliesTotalPages());
    const start = (page - 1) * this.pageSize;
    return this.anomalies().slice(start, start + this.pageSize);
  });

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

    this.hub.on('anomalyDetected', (anomaly: Anomaly) => {
      this.anomalies.update((prev) => [anomaly, ...prev].slice(0, 50));
    });

    this.hub.onclose(() => this.isLive.set(false));
    this.hub.onreconnecting(() => this.isLive.set(false));
    this.hub.onreconnected(() => this.isLive.set(true));

    this.hub.start().then(() => this.isLive.set(true)).catch(() => this.isLive.set(false));
  }

  prevReadingsPage(): void { this.readingsPage$.update(p => p - 1); }
  nextReadingsPage(): void { this.readingsPage$.update(p => p + 1); }
  prevAnomaliesPage(): void { this.anomaliesPage$.update(p => p - 1); }
  nextAnomaliesPage(): void { this.anomaliesPage$.update(p => p + 1); }

  ngOnDestroy(): void {
    this.hub?.stop();
  }
}

bootstrapApplication(AppComponent).catch(console.error);
