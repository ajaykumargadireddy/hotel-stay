import { Pipe, PipeTransform } from '@angular/core';
import { RoomType } from '../models';

@Pipe({
  name: 'roomType',
  standalone: true
})
export class RoomTypePipe implements PipeTransform {
  transform(value: RoomType | number | string | null | undefined): string {
    if (value === null || value === undefined || value === '') {
      return '';
    }

    // If it's already a string enum value, return it
    if (typeof value === 'string' && Object.values(RoomType).includes(value as RoomType)) {
      return value;
    }

    // Convert numeric value to enum string
    const numValue = typeof value === 'string' ? parseInt(value, 10) : value;
    
    switch (numValue) {
      case 0:
        return 'Standard';
      case 1:
        return 'Deluxe';
      case 2:
        return 'Suite';
      default:
        return String(value);
    }
  }
}
