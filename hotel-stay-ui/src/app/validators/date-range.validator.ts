import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export function dateRangeValidator(
  checkInKey = 'checkIn',
  checkOutKey = 'checkOut'
): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const checkIn = control.get(checkInKey)?.value;
    const checkOut = control.get(checkOutKey)?.value;

    if (!checkIn || !checkOut) {
      return null;
    }

    const checkInDate = new Date(checkIn);
    const checkOutDate = new Date(checkOut);

    if (isNaN(checkInDate.getTime()) || isNaN(checkOutDate.getTime())) {
      return null;
    }

    return checkOutDate > checkInDate ? null : { invalidDateRange: true };
  };
}
