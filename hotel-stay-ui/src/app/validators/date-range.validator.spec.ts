import { FormControl, FormGroup } from '@angular/forms';
import { dateRangeValidator } from './date-range.validator';

describe('dateRangeValidator', () => {
  const buildGroup = (checkIn: string, checkOut: string) =>
    new FormGroup({
      checkIn: new FormControl(checkIn),
      checkOut: new FormControl(checkOut)
    }, { validators: [dateRangeValidator()] });

  it('returns error when checkOut <= checkIn', () => {
    const group = buildGroup('2026-08-05', '2026-08-01');
    expect(group.errors).toEqual({ invalidDateRange: true });
  });

  it('returns error when checkOut equals checkIn', () => {
    const group = buildGroup('2026-08-05', '2026-08-05');
    expect(group.errors).toEqual({ invalidDateRange: true });
  });

  it('returns null when checkOut > checkIn', () => {
    const group = buildGroup('2026-08-01', '2026-08-05');
    expect(group.errors).toBeNull();
  });

  it('returns null when dates are missing', () => {
    const group = buildGroup('', '');
    expect(group.errors).toBeNull();
  });
});
