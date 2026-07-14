import { FormControl } from '@angular/forms';
import { pastDateValidator } from './past-date.validator';

describe('pastDateValidator', () => {
  let control: FormControl;
  const validator = pastDateValidator();

  beforeEach(() => {
    control = new FormControl();
  });

  it('should return null for empty value', () => {
    control.setValue('');
    expect(validator(control)).toBeNull();
  });

  it('should return null for null value', () => {
    control.setValue(null);
    expect(validator(control)).toBeNull();
  });

  it('should return null for undefined value', () => {
    control.setValue(undefined);
    expect(validator(control)).toBeNull();
  });

  it('should return null for invalid date string', () => {
    control.setValue('invalid-date');
    expect(validator(control)).toBeNull();
  });

  it('should return null for today\'s date', () => {
    const today = new Date();
    const todayString = today.toISOString().split('T')[0];
    control.setValue(todayString);
    expect(validator(control)).toBeNull();
  });

  it('should return null for future date', () => {
    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);
    const tomorrowString = tomorrow.toISOString().split('T')[0];
    control.setValue(tomorrowString);
    expect(validator(control)).toBeNull();
  });

  it('should return null for date far in the future', () => {
    const futureDate = new Date();
    futureDate.setFullYear(futureDate.getFullYear() + 1);
    const futureDateString = futureDate.toISOString().split('T')[0];
    control.setValue(futureDateString);
    expect(validator(control)).toBeNull();
  });

  it('should return error for yesterday\'s date', () => {
    const yesterday = new Date();
    yesterday.setDate(yesterday.getDate() - 1);
    const yesterdayString = yesterday.toISOString().split('T')[0];
    control.setValue(yesterdayString);
    expect(validator(control)).toEqual({ pastDate: true });
  });

  it('should return error for date in the past', () => {
    const pastDate = new Date();
    pastDate.setDate(pastDate.getDate() - 7);
    const pastDateString = pastDate.toISOString().split('T')[0];
    control.setValue(pastDateString);
    expect(validator(control)).toEqual({ pastDate: true });
  });

  it('should return error for date far in the past', () => {
    const pastDate = new Date();
    pastDate.setFullYear(pastDate.getFullYear() - 1);
    const pastDateString = pastDate.toISOString().split('T')[0];
    control.setValue(pastDateString);
    expect(validator(control)).toEqual({ pastDate: true });
  });

  it('should validate against current date ignoring time', () => {
    // Create a date for today but with different time
    const todayWithTime = new Date();
    todayWithTime.setHours(23, 59, 59, 999);
    const todayString = todayWithTime.toISOString().split('T')[0];
    control.setValue(todayString);
    expect(validator(control)).toBeNull();
  });

  it('should handle ISO date format', () => {
    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);
    const isoString = tomorrow.toISOString();
    control.setValue(isoString);
    expect(validator(control)).toBeNull();
  });

  it('should handle different date formats', () => {
    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);
    const month = String(tomorrow.getMonth() + 1).padStart(2, '0');
    const day = String(tomorrow.getDate()).padStart(2, '0');
    const year = tomorrow.getFullYear();
    
    // MM/DD/YYYY format
    control.setValue(`${month}/${day}/${year}`);
    expect(validator(control)).toBeNull();
  });

  it('should work with Date object', () => {
    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);
    control.setValue(tomorrow);
    expect(validator(control)).toBeNull();
  });

  it('should detect past date with Date object', () => {
    const yesterday = new Date();
    yesterday.setDate(yesterday.getDate() - 1);
    control.setValue(yesterday);
    expect(validator(control)).toEqual({ pastDate: true });
  });

  it('should be case-insensitive for time normalization', () => {
    // Test that today at any time is valid
    const today = new Date();
    today.setHours(12, 30, 45);
    const dateString = today.toISOString().split('T')[0];
    control.setValue(dateString);
    expect(validator(control)).toBeNull();
  });

  it('should handle edge case of date string for yesterday', () => {
    const yesterday = new Date();
    yesterday.setDate(yesterday.getDate() - 1);
    const dateString = yesterday.toISOString().split('T')[0];
    control.setValue(dateString);
    expect(validator(control)).toEqual({ pastDate: true });
  });
});
