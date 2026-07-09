import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges, ChangeDetectionStrategy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { DocumentType, ReservationRequest, RoomWithDetails } from '../../models';
import { documentTypeValidator } from '../../validators/document-type.validator';
import { DocumentTypePipe } from '../../pipes/document-type.pipe';

@Component({
  selector: 'app-reservation-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, DocumentTypePipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './reservation-form.component.html',
  styleUrl: './reservation-form.component.scss'
})
export class ReservationFormComponent implements OnInit, OnChanges {
  private readonly fb = inject(FormBuilder);

  @Input({ required: true }) room!: RoomWithDetails;
  @Input({ required: true }) checkIn!: string;
  @Input({ required: true }) checkOut!: string;
  @Input() isInternational = false;
  @Input() submitting = false;

  @Output() reserve = new EventEmitter<ReservationRequest>();

  readonly DocumentType = DocumentType;

  form = this.fb.group({
    holderName: ['', [Validators.required, Validators.minLength(2)]],
    documentType: [DocumentType.Passport as DocumentType, [Validators.required, documentTypeValidator(() => this.isInternational)]],
    documentNumber: ['', [Validators.required, Validators.minLength(5), Validators.maxLength(20)]]
  });

  get allowedDocumentTypes(): DocumentType[] {
    return this.isInternational
      ? [DocumentType.Passport]
      : [DocumentType.Passport, DocumentType.NationalId];
  }

  ngOnInit(): void {
    this.applyDocumentTypeConstraints();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['isInternational']) {
      this.applyDocumentTypeConstraints();
    }
  }

  private applyDocumentTypeConstraints(): void {
    if (this.isInternational && this.form.controls.documentType.value !== DocumentType.Passport) {
      this.form.controls.documentType.setValue(DocumentType.Passport);
    }
    this.form.controls.documentType.updateValueAndValidity();
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    this.reserve.emit({
      roomId: this.room.roomId,
      checkIn: this.checkIn,
      checkOut: this.checkOut,
      document: {
        holderName: value.holderName!,
        type: value.documentType!,
        number: value.documentNumber!
      }
    });
  }
}
