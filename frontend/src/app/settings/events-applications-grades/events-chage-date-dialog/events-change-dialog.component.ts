import { Component, Inject, EventEmitter, Output } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';

@Component({
  selector: 'app-events-change-dialog',
  templateUrl: './events-change-dialog.component.html',
  styleUrls: ['./events-change-dialog.component.scss']
})
export class EventsChangeDialogComponent {

public eventData: any;
public currentSchedule: any;
public selectedScheduleId: string;
@Output() changeDateOfEvents = new EventEmitter();

  constructor(
    public dialogRef: MatDialogRef<EventsChangeDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public event: any
  ) {
    this.eventData = event.data;
    this.eventData.allSchedules = event.data.allSchedules.filter(x => x.id !== event.data.scheduleId);
    this.currentSchedule = event.data.event.schedules.find(x => x.id === event.data.scheduleId);
  }
  public dismiss() {
    this.dialogRef.close(false);
  }
  public changeSchedule() {
    this.dialogRef.close(this.selectedScheduleId);
  }

}
