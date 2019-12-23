import { Component, Output, EventEmitter, Input } from '@angular/core';

@Component({
  selector: 'app-report-area-toggle',
  templateUrl: './report-area-toggle.html',
  styleUrls: ['./report-area-toggle.component.scss']
})
export class ReportAreaToggleComponent {

  @Input() readonly title: string;
  @Input() readonly isClosed: boolean;
  @Output() toggle: EventEmitter<boolean> = new EventEmitter();

}
