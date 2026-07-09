import { Component, EventEmitter, Input, Output, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RoomWithDetails } from '../../models';
import { RoomTypePipe } from '../../pipes/room-type.pipe';

@Component({
  selector: 'app-room-card',
  standalone: true,
  imports: [CommonModule, RoomTypePipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './room-card.component.html',
  styleUrl: './room-card.component.scss'
})
export class RoomCardComponent {
  @Input({ required: true }) room!: RoomWithDetails;
  @Output() book = new EventEmitter<RoomWithDetails>();

  onBook(): void {
    this.book.emit(this.room);
  }

  get stars(): number[] {
    return this.room.starRating ? Array.from({ length: this.room.starRating }, (_, i) => i) : [];
  }
}
