import { Component, Input, AfterViewInit } from '@angular/core';
import * as Chart from 'chart.js';

@Component({
  selector: 'app-recommendation-radar',
  template: `
    <div class="performance" >
      <div class="content" >
        <div class="totals"  >
          <div class="legends" >
            <ng-container *ngFor="let data of dataset">
              <div class="legend" >
                <div class="square" [ngStyle]="{'background': data.backgroundColor}"></div>
                {{data.label}}
              </div>
            </ng-container>
          </div>
        </div>
        <canvas
          [id]="canvasId" width="95%" height="95%"
        ></canvas>
      </div>
    </div>`,
  styleUrls: ['./recommendation-radar.component.scss']
})
export class RecommendationRadarComponent {

  @Input() readonly canvasId: string;
  @Input() set setCanvasId(id: string) {
    setTimeout(() => { this._createChart(id); }, 100);
  }
  @Input() readonly labels: Array<number>;
  @Input() readonly titleCallback: Function;
  @Input() readonly dataset: any;
  @Input() readonly tooltipCallback: Function;

  public chart: any;

  private _createChart(chartId: string) {
    this.chart = new Chart( chartId, {
      type: 'radar',
      data: {
        labels: this.labels,
        datasets: this.dataset
      },
      options: {
        legend: { display: false },
        tooltips: {
          enabled: true,
          titleFontSize: 20,
          bodyFontSize: 24,
          callbacks: {
            title: this.titleCallback,
            label: this.tooltipCallback
          }
        },
        scale: {
          ticks: {
            display: true,
            min: 0,
            max: 4
          },
          pointLabels: {
            fontSize: 26,
            fontColor: '#c4c4c4',
            fontStyle: 'bold'
          }
        },
        scales: {
          xAxes: [{ display: false }],
          yAxes: [{ display: false }],
        }
      }
    });
  }
}
