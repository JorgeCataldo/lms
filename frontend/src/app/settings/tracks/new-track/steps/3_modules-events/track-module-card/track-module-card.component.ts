import { Component, Input, Output, EventEmitter, ViewChild } from '@angular/core';
import { TrackModule } from '../../../../../../models/track-module.model';
import { TrackEvent } from '../../../../../../models/track-event.model';
import { Level } from '../../../../../../models/shared/level.interface';

@Component({
  selector: 'app-track-module-card',
  templateUrl: './track-module-card.component.html',
  styleUrls: ['./track-module-card.component.scss']
})
export class TrackModuleCardComponent {

  @Input() readonly trackModule: TrackModule | TrackEvent;
  @Input() readonly modulesLength: number;
  @Input() readonly levels: Array<Level> = [];
  @Output() updateItemsOrder = new EventEmitter<Array<number>>();
  @Output() removeItem = new EventEmitter<number>();

  public getLevelDescription(levelId: number): string {
    const level = this.levels.find(lev => lev.id === levelId);
    return level ? level.description : '';
  }

  public changeModuleOrder(selectedOrder: number) {
    if (selectedOrder !== this.trackModule.order) {
      const orderArray = [ this.trackModule.order, selectedOrder ];
      this.updateItemsOrder.emit( orderArray );
    }
  }

  public removeModuleFromTrack(selectedOrder: number) {
    this.removeItem.emit( selectedOrder );
  }
}
