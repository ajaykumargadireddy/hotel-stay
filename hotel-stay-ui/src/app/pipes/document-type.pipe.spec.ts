import { DocumentTypePipe } from './document-type.pipe';
import { DocumentType } from '../models';

describe('DocumentTypePipe', () => {
  let pipe: DocumentTypePipe;

  beforeEach(() => {
    pipe = new DocumentTypePipe();
  });

  it('should create an instance', () => {
    expect(pipe).toBeTruthy();
  });

  it('should convert numeric value 0 to Passport', () => {
    expect(pipe.transform(0)).toBe('Passport');
  });

  it('should convert numeric value 1 to National ID', () => {
    expect(pipe.transform(1)).toBe('National ID');
  });

  it('should handle string enum value Passport', () => {
    expect(pipe.transform(DocumentType.Passport)).toBe('Passport');
  });

  it('should handle string enum value NationalId', () => {
    expect(pipe.transform(DocumentType.NationalId)).toBe('National ID');
  });

  it('should handle string "NationalId"', () => {
    expect(pipe.transform('NationalId')).toBe('National ID');
  });

  it('should handle string numeric values', () => {
    expect(pipe.transform('0')).toBe('Passport');
    expect(pipe.transform('1')).toBe('National ID');
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
