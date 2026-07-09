import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';
import { DocumentType } from '../models';

/**
 * Returns a validator that ensures international destinations only accept Passport.
 * Domestic destinations accept both Passport and NationalId.
 */
export function documentTypeValidator(isInternational: () => boolean): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = control.value as DocumentType | null;
    if (!value) {
      return null;
    }

    if (isInternational() && value !== DocumentType.Passport) {
      return { invalidDocumentType: true };
    }

    return null;
  };
}

/**
 * Domestic country code (India). All other countries are considered international.
 */
export const DOMESTIC_COUNTRY_CODES = ['IN'];

export function isInternationalCountry(countryCode: string | null | undefined): boolean {
  if (!countryCode) {
    return false;
  }
  return !DOMESTIC_COUNTRY_CODES.includes(countryCode.toUpperCase());
}
