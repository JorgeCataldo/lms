import { Component, Input } from '@angular/core';
import { ModulePreview } from '../../../../models/previews/module.interface';
import { TrackPreview } from '../../../../models/previews/track.interface';
import { EventPreview } from '../../../../models/previews/event.interface';

@Component({
  selector: 'app-user-details-progress',
  templateUrl: './user-progress.component.html',
  styleUrls: ['./user-progress.component.scss']
})
export class SettingsUserDetailsProgressComponent {

  @Input() modules: any; // ModulePreview;
  @Input() tracks: any; // TrackPreview;
  @Input() events: any; // EventPreview;

}
