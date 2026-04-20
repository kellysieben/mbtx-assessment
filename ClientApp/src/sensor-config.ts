/**
 * Thresholds for sensor status colour coding.
 * Status is determined as:
 *   green  — value is within normal operating range
 *   yellow — value is outside normal range but not critical
 *   red    — value is at a critical level
 */
export const SENSOR_LIMITS = {
  temperature: {
    // °C
    greenMax: 28,   // green:  ≤ 28 °C
    yellowMax: 35,  // yellow: 28–35 °C  |  red: > 35 °C
  },
  humidity: {
    // %
    greenMin: 40,   // green:  40–70 %
    greenMax: 70,
    yellowMin: 20,  // yellow: 20–40 % or 70–85 %  |  red: < 20 % or > 85 %
    yellowMax: 85,
  },
  co2Ppm: {
    // ppm
    greenMax: 1000,  // green:  ≤ 1000 ppm
    yellowMax: 2000, // yellow: 1000–2000 ppm  |  red: > 2000 ppm
  },
};
