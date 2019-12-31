import { Component, Input } from '@angular/core';
import * as Chart from 'chart.js';

@Component({
  selector: 'app-recommendation-bar',
  template: `
    <div class="performance" >
      <div class="content" >
        <canvas
          [id]="canvasId"
        ></canvas>
      </div>
    </div>`,
  styleUrls: ['./recommendation-bar.component.scss']
})
export class RecommendationBarComponent {

  @Input() readonly canvasId: string;
  @Input() set setCanvasId(id: string) {
    setTimeout(() => { this._createChart(id); }, 100);
  }
  @Input() readonly labels: any;
  @Input() readonly dataset: any;

  public chart: any;

  private _createChart(chartId: string) {
    this.chart = new Chart( chartId, {
      type: 'bar',
      data: {
        labels: this.labels,
        datasets: this.dataset
      },
      options: {
        scales: {
          yAxes: [{
            ticks: {
              reverse: false,
              min: 0,
              max: 10
            }
          }]
        }
      }
    });
  }
}
