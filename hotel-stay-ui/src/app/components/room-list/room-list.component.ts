import { Component, EventEmitter, Input, Output, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RoomWithDetails } from '../../models';
import { RoomCardComponent } from '../room-card/room-card.component';

type SortOrder = 'asc' | 'desc';

@Component({
  selector: 'app-room-list',
  standalone: true,
  imports: [CommonModule, RoomCardComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './room-list.component.html',
  styleUrl: './room-list.component.scss'
})
export class RoomListComponent {
  @Input() rooms: RoomWithDetails[] = [];
  @Input() loading = false;
  @Input() hasSearched = false;
  @Output() book = new EventEmitter<RoomWithDetails>();

  sortOrder: SortOrder = 'asc';

  get sortedRooms(): RoomWithDetails[] {
    const sorted = [...this.rooms];
    sorted.sort((a, b) =>
      this.sortOrder === 'asc' ? a.totalPrice - b.totalPrice : b.totalPrice - a.totalPrice
    );
    return sorted;
  }

  toggleSort(): void {
    this.sortOrder = this.sortOrder === 'asc' ? 'desc' : 'asc';
  }

  trackByRoomId(_: number, room: RoomWithDetails): string {
    return room.roomId;
  }
}
