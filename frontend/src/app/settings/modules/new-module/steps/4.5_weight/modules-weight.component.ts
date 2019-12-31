import { Component, Input, Output, EventEmitter } from '@angular/core';
import { MatSnackBar } from '@angular/material';
import { NotificationClass } from '../../../../../shared/classes/notification';
import { TrackEvent } from '../../../../../models/track-event.model';
import { ModuleWeights } from 'src/app/models/module.model';


@Component({
  selector: 'app-new-modules-weight',
  templateUrl: './modules-weight.component.html',
  styleUrls: ['./modules-weight.component.scss']
})
export class NewModulesWeightComponent extends NotificationClass {

  public readonly displayedColumns: string[] = [
    'content', 'weight'
  ];
  @Output() addModulesWeights = new EventEmitter<Array<Array<any>>>();
  @Input() weight: ModuleWeights[];
  @Input() totalWeight: number = 0;

  public events: Array<TrackEvent> = [];

  public moduleWeights: ModuleWeights [];


  constructor(
    protected _snackBar: MatSnackBar
  ) {
    super(_snackBar);

  }

  public nextStep(): void {
      if (this.totalWeight === 100) {
        this.addModulesWeights.emit( [ this.weight ] );
      } else {
        console.log(this.weight);
        this.notify('A soma dos pesos dos itens deve dar 100');
      }
  }

  public getCurrentProgress(): string {
    return this.totalWeight > 100 ? (0).toString() + '%' :
      (100 - this.totalWeight).toString() + '%';
  }

  public setTotalValue() {
    this.totalWeight = 0;
    const weights = this.weight.map(x => x.weight);
    weights.forEach(weight => {
      this.totalWeight += weight ? weight : 0;
    });
  }


  public setRowValue(value: number, row: ModuleWeights) {

    if (value < 0) {
      this.notify('Valores negativos não são permitidos.');
      row.weight = 0;
      return false;
    }

    row.weight = +value;
    this.setTotalValue();
  }

}
