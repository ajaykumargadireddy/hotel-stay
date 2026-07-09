import { RoomTypePipe } from './room-type.pipe';
import { RoomType } from '../models';

describe('RoomTypePipe', () => {
  let pipe: RoomTypePipe;

  beforeEach(() => {
    pipe = new RoomTypePipe();
  });

  it('should create an instance', () => {
    expect(pipe).toBeTruthy();
  });

  it('should convert numeric value 0 to Standard', () => {
    expect(pipe.transform(0)).toBe('Standard');
  });

  it('should convert numeric value 1 to Deluxe', () => {
    expect(pipe.transform(1)).toBe('Deluxe');
  });

  it('should convert numeric value 2 to Suite', () => {
    expect(pipe.transform(2)).toBe('Suite');
  });

  it('should handle string enum values', () => {
    expect(pipe.transform(RoomType.Standard)).toBe('Standard');
    expect(pipe.transform(RoomType.Deluxe)).toBe('Deluxe');
    expect(pipe.transform(RoomType.Suite)).toBe('Suite');
  });

  it('should handle string numeric values', () => {
    expect(pipe.transform('0')).toBe('Standard');
    expect(pipe.transform('1')).toBe('Deluxe');
    expect(pipe.transform('2')).toBe('Suite');
  });

  it('should return empty string for null', () => {
    expect(pipe.transform(null)).toBe('');
  });

  it('should return empty string for undefined', () => {
    expect(pipe.transform(undefined)).toBe('');
  });

  it('should return empty string for empty string', () => {
    expect(pipe.transform('')).toBe('');
  });

  it('should return original value as string for unknown values', () => {
    expect(pipe.transform(99)).toBe('99');
  });
});
