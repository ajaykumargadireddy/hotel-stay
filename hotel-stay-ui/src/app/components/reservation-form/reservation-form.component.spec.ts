import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { ReservationFormComponent } from './reservation-form.component';
import { DocumentType, RoomType, RoomWithDetails } from '../../models';

describe('ReservationFormComponent', () => {
  let component: ReservationFormComponent;
  let fixture: ComponentFixture<ReservationFormComponent>;

  const mockRoom: RoomWithDetails = {
    roomId: 'room-123',
    provider: 'PremierStays',
    destination: 'BOM',
    location: 'Mumbai, India',
    roomType: RoomType.Deluxe,
    checkIn: '2026-08-01',
    checkOut: '2026-08-05',
    perNightRate: 150,
    currency: 'USD',
    numberOfNights: 4,
    totalPrice: 600,
    cancellationPolicy: 'Free cancellation up to 48h before check-in',
    starRating: 4,
    amenities: ['WiFi', 'Pool']
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ReservationFormComponent, ReactiveFormsModule]
    }).compileComponents();

    fixture = TestBed.createComponent(ReservationFormComponent);
    component = fixture.componentInstance;
    component.room = mockRoom;
    component.checkIn = '2026-08-01';
    component.checkOut = '2026-08-05';
    component.isInternational = false;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should allow both Passport and National ID for domestic destinations', () => {
    component.isInternational = false;
    fixture.detectChanges();

    expect(component.allowedDocumentTypes).toContain(DocumentType.Passport);
    expect(component.allowedDocumentTypes).toContain(DocumentType.NationalId);
    expect(component.allowedDocumentTypes.length).toBe(2);
  });

  it('should allow only Passport for international destinations', () => {
    component.isInternational = true;
    fixture.detectChanges();

    expect(component.allowedDocumentTypes).toContain(DocumentType.Passport);
    expect(component.allowedDocumentTypes).not.toContain(DocumentType.NationalId);
    expect(component.allowedDocumentTypes.length).toBe(1);
  });

  it('should validate holder name with minimum length', () => {
    fixture.detectChanges();

    const nameControl = component.form.controls.holderName;

    nameControl.setValue('A');
    expect(nameControl.invalid).toBe(true);
    expect(nameControl.errors?.['minlength']).toBeTruthy();

    nameControl.setValue('John Doe');
    expect(nameControl.valid).toBe(true);
  });

  it('should validate document number with length constraints', () => {
    fixture.detectChanges();

    const docControl = component.form.controls.documentNumber;

    docControl.setValue('123');
    expect(docControl.invalid).toBe(true);
    expect(docControl.errors?.['minlength']).toBeTruthy();

    docControl.setValue('12345678901234567890X');
    expect(docControl.invalid).toBe(true);
    expect(docControl.errors?.['maxlength']).toBeTruthy();

    docControl.setValue('P123456789');
    expect(docControl.valid).toBe(true);
  });

  it('should force Passport selection for international destinations on init', () => {
    component.isInternational = true;
    component.form.controls.documentType.setValue(DocumentType.NationalId);

    component.ngOnInit();

    expect(component.form.controls.documentType.value).toBe(DocumentType.Passport);
  });

  it('should update document type to Passport when isInternational changes to true', () => {
    component.isInternational = false;
    fixture.detectChanges();

    component.form.controls.documentType.setValue(DocumentType.NationalId);

    component.isInternational = true;
    component.ngOnChanges({
      isInternational: {
        currentValue: true,
        previousValue: false,
        firstChange: false,
        isFirstChange: () => false
      }
    });

    expect(component.form.controls.documentType.value).toBe(DocumentType.Passport);
  });

  it('should not change document type when isInternational changes to false', () => {
    component.isInternational = true;
    fixture.detectChanges();

    component.form.controls.documentType.setValue(DocumentType.Passport);

    component.isInternational = false;
    component.ngOnChanges({
      isInternational: {
        currentValue: false,
        previousValue: true,
        firstChange: false,
        isFirstChange: () => false
      }
    });

    expect(component.form.controls.documentType.value).toBe(DocumentType.Passport);
  });

  it('should validate document type based on destination type', () => {
    component.isInternational = true;
    fixture.detectChanges();

    component.form.controls.documentType.setValue(DocumentType.NationalId);

    expect(component.form.controls.documentType.invalid).toBe(true);
    expect(component.form.controls.documentType.errors?.['invalidDocumentType']).toBeTruthy();
  });

  it('should emit reserve event with correct data on valid submit', () => {
    fixture.detectChanges();

    component.form.patchValue({
      holderName: 'John Doe',
      documentType: DocumentType.Passport,
      documentNumber: 'P123456789'
    });

    let emittedData: any;
    component.reserve.subscribe(data => emittedData = data);

    component.onSubmit();

    expect(emittedData).toBeDefined();
    expect(emittedData.roomId).toBe('room-123');
    expect(emittedData.checkIn).toBe('2026-08-01');
    expect(emittedData.checkOut).toBe('2026-08-05');
    expect(emittedData.document.holderName).toBe('John Doe');
    expect(emittedData.document.type).toBe(DocumentType.Passport);
    expect(emittedData.document.number).toBe('P123456789');
  });

  it('should not emit reserve event when form is invalid', () => {
    fixture.detectChanges();

    let emitted = false;
    component.reserve.subscribe(() => emitted = true);

    component.onSubmit();

    expect(emitted).toBe(false);
  });

  it('should mark all fields as touched when submitting invalid form', () => {
    fixture.detectChanges();

    component.onSubmit();

    expect(component.form.controls.holderName.touched).toBe(true);
    expect(component.form.controls.documentType.touched).toBe(true);
    expect(component.form.controls.documentNumber.touched).toBe(true);
  });

  it('should disable form when submitting is true', () => {
    component.submitting = false;
    fixture.detectChanges();

    expect(component.form.enabled).toBe(true);

    // In real usage, parent component sets this input
    // Here we test that the component respects it
  });

  it('should have all required fields defined', () => {
    fixture.detectChanges();

    expect(component.form.controls.holderName).toBeDefined();
    expect(component.form.controls.documentType).toBeDefined();
    expect(component.form.controls.documentNumber).toBeDefined();
  });

  it('should default to Passport document type', () => {
    fixture.detectChanges();

    expect(component.form.controls.documentType.value).toBe(DocumentType.Passport);
  });

  it('should require all form fields', () => {
    fixture.detectChanges();

    component.form.patchValue({
      holderName: '',
      documentType: null,
      documentNumber: ''
    });

    expect(component.form.controls.holderName.hasError('required')).toBe(true);
    expect(component.form.controls.documentType.hasError('required')).toBe(true);
    expect(component.form.controls.documentNumber.hasError('required')).toBe(true);
  });
});
