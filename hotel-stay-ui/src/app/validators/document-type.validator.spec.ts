import { FormControl } from '@angular/forms';
import { documentTypeValidator, isInternationalCountry } from './document-type.validator';
import { DocumentType } from '../models';

describe('documentTypeValidator', () => {
  it('returns error for international destination with NationalId', () => {
    const validator = documentTypeValidator(() => true);
    const control = new FormControl(DocumentType.NationalId);
    expect(validator(control)).toEqual({ invalidDocumentType: true });
  });

  it('returns null for international destination with Passport', () => {
    const validator = documentTypeValidator(() => true);
    const control = new FormControl(DocumentType.Passport);
    expect(validator(control)).toBeNull();
  });

  it('returns null for domestic destination with NationalId', () => {
    const validator = documentTypeValidator(() => false);
    const control = new FormControl(DocumentType.NationalId);
    expect(validator(control)).toBeNull();
  });

  it('returns null for domestic destination with Passport', () => {
    const validator = documentTypeValidator(() => false);
    const control = new FormControl(DocumentType.Passport);
    expect(validator(control)).toBeNull();
  });

  it('returns null when value is empty', () => {
    const validator = documentTypeValidator(() => true);
    const control = new FormControl('');
    expect(validator(control)).toBeNull();
  });
});

describe('isInternationalCountry', () => {
  it('returns false for India (IN)', () => {
    expect(isInternationalCountry('IN')).toBe(false);
  });

  it('returns true for UK (GB)', () => {
    expect(isInternationalCountry('GB')).toBe(true);
  });

  it('returns true for US', () => {
    expect(isInternationalCountry('US')).toBe(true);
  });

  it('is case insensitive', () => {
    expect(isInternationalCountry('in')).toBe(false);
  });

  it('returns false for missing country', () => {
    expect(isInternationalCountry(null)).toBe(false);
    expect(isInternationalCountry('')).toBe(false);
  });
});
