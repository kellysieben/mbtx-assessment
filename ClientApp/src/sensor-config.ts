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
    greenMax: 21,   // green:  ≤ 21 °C
    yellowMax: 30,  // yellow: 21–30 °C  |  red: > 30 °C
  },
  humidity: {
    // %
    greenMin: 57,   // green:  57-65 %
    greenMax: 62,
    yellowMin: 40,  // yellow: 40–57 % or 62–75 %  |  red: < 40 % or > 75 %
    yellowMax: 75,
  },
  co2Ppm: {
    // ppm
    greenMax: 620,  // green:  ≤ 620 ppm
    yellowMax: 650, // yellow: 620–650 ppm  |  red: > 650 ppm
  },
};
