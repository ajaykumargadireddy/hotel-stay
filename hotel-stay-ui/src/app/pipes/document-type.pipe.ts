import { Pipe, PipeTransform } from '@angular/core';
import { DocumentType } from '../models';

@Pipe({
  name: 'documentType',
  standalone: true
})
export class DocumentTypePipe implements PipeTransform {
  transform(value: DocumentType | number | string | null | undefined): string {
    if (value === null || value === undefined || value === '') {
      return '';
    }

    // If it's already a string enum value, format it
    if (typeof value === 'string') {
      if (value === DocumentType.NationalId || value === 'NationalId') {
        return 'National ID';
      }
      if (value === DocumentType.Passport || value === 'Passport') {
        return 'Passport';
      }
      return value;
    }

    // Convert numeric value to enum string
    const numValue = typeof value === 'string' ? parseInt(value, 10) : value;
    
    switch (numValue) {
      case 0:
        return 'Passport';
      case 1:
        return 'National ID';
      default:
        return String(value);
    }
  }
}
