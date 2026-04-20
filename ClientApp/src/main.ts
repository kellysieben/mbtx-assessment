import { Component, OnInit, OnDestroy, signal, computed, inject } from '@angular/core';
import { bootstrapApplication } from '@angular/platform-browser';
import { CommonModule } from '@angular/common';
import { HttpClient, provideHttpClient } from '@angular/common/http';
import * as signalR from '@microsoft/signalr';
import { SENSOR_LIMITS } from './sensor-config';

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
      <h1>Greenhouse Monitor</h1>

      <!-- Status Indicator -->
      <div class="status-bar">
        <div class="status" [class.live]="isLive()" [class.offline]="!isLive()">
          {{ isLive() ? 'LIVE' : 'OFFLINE' }}
        </div>
        <span class="last-update" *ngIf="lastReadingTimestamp()">
          Last reading: {{ lastReadingTimestamp() | date:'yyyy-MM-dd @ HH:mm:ss' }}
        </span>
      </div>

      <!-- Sensor Cards -->
      <div class="sensor-cards">
        <div class="sensor-card" [ngClass]="'status-' + tempStatus()">
          <div class="card-label">Temperature</div>
          <div class="card-value" *ngIf="latestReading()">{{ latestReading()!.temperature | number:'1.1-1' }} °C</div>
          <div class="card-value card-no-data" *ngIf="!latestReading()">—</div>
          <div class="card-unit">degrees Celsius</div>
        </div>
        <div class="sensor-card" [ngClass]="'status-' + humidityStatus()">
          <div class="card-label">Humidity</div>
          <div class="card-value" *ngIf="latestReading()">{{ latestReading()!.humidity | number:'1.1-1' }} %</div>
          <div class="card-value card-no-data" *ngIf="!latestReading()">—</div>
          <div class="card-unit">relative humidity</div>
        </div>
        <div class="sensor-card" [ngClass]="'status-' + co2Status()">
          <div class="card-label">CO&#x2082;</div>
          <div class="card-value" *ngIf="latestReading()">{{ latestReading()!.co2Ppm | number:'1.0-0' }} ppm</div>
          <div class="card-value card-no-data" *ngIf="!latestReading()">—</div>
          <div class="card-unit">parts per million</div>
        </div>
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

  private readonly lastReadingKey = 'lastSensorReadingTimestamp';

  isLive = signal(false);
  readings = signal<SensorReading[]>([]);
  anomalies = signal<Anomaly[]>([]);
  latestReading = signal<SensorReading | null>(null);
  lastReadingTimestamp = signal<string | null>(localStorage.getItem(this.lastReadingKey));

  tempStatus = computed(() => {
    const r = this.latestReading();
    if (!r) return 'none';
    const t = Number(r.temperature);
    if (t <= SENSOR_LIMITS.temperature.greenMax) return 'green';
    if (t <= SENSOR_LIMITS.temperature.yellowMax) return 'yellow';
    return 'red';
  });

  humidityStatus = computed(() => {
    const r = this.latestReading();
    if (!r) return 'none';
    const h = Number(r.humidity);
    if (h >= SENSOR_LIMITS.humidity.greenMin && h <= SENSOR_LIMITS.humidity.greenMax) return 'green';
    if (h >= SENSOR_LIMITS.humidity.yellowMin && h <= SENSOR_LIMITS.humidity.yellowMax) return 'yellow';
    return 'red';
  });

  co2Status = computed(() => {
    const r = this.latestReading();
    if (!r) return 'none';
    const c = r.co2Ppm;
    if (c <= SENSOR_LIMITS.co2Ppm.greenMax) return 'green';
    if (c <= SENSOR_LIMITS.co2Ppm.yellowMax) return 'yellow';
    return 'red';
  });

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
  private http = inject(HttpClient);

  ngOnInit(): void {
    this.loadInitialAnomalies();
    this.loadLatestReading();
    this.connectHub();
  }

  private loadInitialAnomalies(): void {
    this.http.get<Anomaly[]>('/api/anomaly?limit=20').subscribe({
      next: (data) => this.anomalies.set(data),
      error: (err) => console.error('Failed to load initial anomalies', err),
    });
  }

  private loadLatestReading(): void {
    this.http.get<SensorReading>('/api/readings/latest').subscribe({
      next: (data) => this.latestReading.set(data),
      error: (err) => console.error('Failed to load latest reading', err),
    });
  }

  private connectHub(): void {
    this.hub = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/sensor')
      .withAutomaticReconnect()
      .build();

    this.hub.on('sensorReadingAvailable', (reading: SensorReading) => {
      this.readings.update((prev) => [reading, ...prev].slice(0, 50));
      this.latestReading.set(reading);
      this.lastReadingTimestamp.set(reading.timestamp);
      localStorage.setItem(this.lastReadingKey, reading.timestamp);
    });

    this.hub.on('anomalyDetected', (anomaly: Anomaly) => {
      this.anomalies.update((prev) =>
        prev.some(a => a.id === anomaly.id) ? prev : [anomaly, ...prev].slice(0, 50)
      );
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

bootstrapApplication(AppComponent, {
  providers: [provideHttpClient()],
}).catch(console.error);
