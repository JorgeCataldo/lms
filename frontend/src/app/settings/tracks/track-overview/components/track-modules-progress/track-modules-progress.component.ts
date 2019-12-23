import { Component, Input, AfterViewInit, OnInit } from '@angular/core';
import { TrackStudentOverview } from 'src/app/models/track-overview.interface';
import * as Chart from 'chart.js';
import { MatSnackBar } from '@angular/material';
import { SettingsTracksService } from 'src/app/settings/_services/tracks.service';
import { NotificationClass } from 'src/app/shared/classes/notification';
import { TrackUserProgress } from 'src/app/models/track-user-progress.model';

@Component({
  selector: 'app-track-modules-progress',
  templateUrl: './track-modules-progress.component.html',
  styleUrls: ['./track-modules-progress.component.scss']
})
export class TrackModulesProgressGanttComponent extends NotificationClass implements OnInit {

  @Input() readonly title: string;
  @Input() readonly track: TrackStudentOverview;
  @Input() readonly labels: Array<number>;
  @Input() readonly dataset: any;
  @Input() readonly titleCallback: Function;
  @Input() readonly tooltipCallback: Function;
  @Input() readonly showLegends: boolean = true;
  @Input() readonly totalDays: number;
  @Input() readonly expectedProgress: number;

  public stackedChart;
  // public users: Array<TrackUserProgress> = [];
  public users: Array<TrackUserProgress> = [];
  public readonly displayedColumns: string[] = [
   'title', 'started', 'finished'
  ];

  public readonly eventsDisplayedColumns: string[] = [
    'title', 'applyed', 'date'
   ];

  public panelOpenState = false;

  constructor(
    protected _snackBar: MatSnackBar,
    private _tracksService: SettingsTracksService,
  ) {
    super(_snackBar);
  }

// ngAfterViewInit

  ngOnInit() {

    this._getUsersProgress(this.track.id);

    this.stackedChart = new Chart('stackedProgressCanvas', {
      type: 'horizontalBar',
      data: {
        labels: this.labels,
        datasets: this.dataset
      },
      options: {
        legend: {
          display: true
        },
        annotation: {
          annotations: [{
            type: 'line',
            mode: 'vertical',
            scaleID: 'y-axis-0',
            value: 5,
            borderColor: 'rgb(75, 192, 192)',
            borderWidth: 4,
            label: {
              enabled: false,
              content: 'Test label'
            }
          }]
        },
        tooltips: {
          enabled: false,
          titleFontSize: 15,
          bodyFontSize: 10,
          // callbacks: {
          //   title: this.titleCallback,
          //   label: this.tooltipCallback
          // }
          custom: function(tooltipModel) {
            // Tooltip Element
            let tooltipEl = document.getElementById('chartjs-tooltip');

            // Create element on first render
            if (!tooltipEl) {
                tooltipEl = document.createElement('div');
                tooltipEl.id = 'chartjs-tooltip';
                tooltipEl.innerHTML = '<table></table>';
                document.body.appendChild(tooltipEl);
            }

            // Hide if no tooltip
            if (tooltipModel.opacity === 0) {
                tooltipEl.style.opacity = '0';
                return;
            }

            // Set caret Position
            tooltipEl.classList.remove('above', 'below', 'no-transform');
            if (tooltipModel.yAlign) {
                tooltipEl.classList.add(tooltipModel.yAlign);
            } else {
                tooltipEl.classList.add('no-transform');
            }

            function getBody(bodyItem) {
                return bodyItem.lines;
            }
            tooltipModel.body = null;
            // Set Text
            if (tooltipModel.body) {
              const titleLines = tooltipModel.title || [];
              const bodyLines = tooltipModel.body.map(getBody);

              let innerHtml = '<thead>';

                titleLines.forEach(function(title) {
                    innerHtml += '<tr><th>' + title + '</th></tr>';
                });
                innerHtml += '</thead><tbody>';
                innerHtml += '<tr><td>teste</td></tr>';
              //   bodyLines.forEach(function(body, i) {
              //     if (i > 0) {
              //     const colors = tooltipModel.labelColors[i];
              //     let style = 'background:' + colors.backgroundColor;
              //       style += '; border-color:' + colors.borderColor;
              //       style += '; border-width: 2px';
              //       const span = '<span style="' + style + '"></span>';
              //       innerHtml += '<tr><td>' + span + body + '</td></tr>';
              // }
              //   });
                innerHtml += '</tbody>';

                const tableRoot = tooltipEl.querySelector('table');
                tableRoot.innerHTML = innerHtml;
            }

            // `this` will be the overall tooltip
            const position = this._chart.canvas.getBoundingClientRect();

            // Display, position, and set styles for font
            tooltipEl.style.opacity = '1';
            tooltipEl.style.position = 'absolute';
            tooltipEl.style.left = position.left + window.pageXOffset + tooltipModel.caretX + 'px';
            tooltipEl.style.top = position.top + window.pageYOffset + tooltipModel.caretY + 'px';
            tooltipEl.style.fontFamily = tooltipModel._bodyFontFamily;
            tooltipEl.style.fontSize = tooltipModel.bodyFontSize + 'px';
            tooltipEl.style.fontStyle = tooltipModel._bodyFontStyle;
            tooltipEl.style.padding = tooltipModel.yPadding + 'px ' + tooltipModel.xPadding + 'px';
            tooltipEl.style.pointerEvents = 'none';
        },
        // animation: {
        //   onComplete: function () {
        //     const chartInstance = this.chart;
        //     const ctx = chartInstance.ctx;
        //     ctx.textAlign = 'right';
        //     ctx.font = 'bold 10px';
        //     ctx.fillStyle = '#3c3c3c';

        //     Chart.helpers.each(this.data.datasets.forEach((dataset, i) => {
        //         const meta = chartInstance.controller.getDatasetMeta(i);
        //         Chart.helpers.each(meta.data.forEach((bar, index) => {
        //             const data = dataset.data[index];
        //             if (data > 4) {
        //               ctx.fillText(
        //                 data + '%',
        //                 bar._model.base + 15 + ((bar._model.x - bar._model.base) / 2),
        //                 bar._model.y + 4
        //               );
        //             }
        //         }), this);
        //     }), this);
        //   }
        // },
      },
        scales: {
          xAxes: [{
            display: true,
            stacked: true
          }],
          yAxes: [{
            ticks: {
              beginAtZero: true,
              fontSize: 15
            },
            stacked: true,
            barPercentage: 0.7
          }],
        }
      }
    });

    this.stackedChart.generateLegend();
    this.stackedChart.update(true);
  }

  private _getUsersProgress(trackId: string): void {
    this._tracksService.getUsersProgress(
      trackId
    ).subscribe((response) => {
      this.users = response.data;
      const today = new Date();

      for (let i = 0; i < this.users.length; i++) {
        this.users[i].currentProgress = parseFloat(this.users[i].currentProgress.toFixed(2));
        this.users[i].finalGrade = parseFloat(this.users[i].finalGrade.toFixed(2));

        if (this.users[i].currentProgress <= 79.72) {
          this.users[i].progressColor = 'low-progress';
        } else {
          if (this.users[i].currentProgress >= 99.65) {
            this.users[i].progressColor = 'high-progress';
          } else {
            this.users[i].progressColor = 'average-progress';
          }
        }

        for (let k = 0; k < this.users[i].modules.length; k++) {
          this.users[i].modules[k].moduleStatus = 1;
          this.users[i].modules[k].userModuleStatus = 1;
          /*
            Modulo Fechado        = 1 // moduleStatus
            Modulo Aberto         = 2 // moduleStatus
            Modulo Encerrado      = 3 // moduleStatus

            Modulo Não Iniciado   = 1 // userModuleStatus
            Modulo Em Andamento   = 2 // userModuleStatus
            Modulo Finalizado     = 3 // userModuleStatus
          */

          this.users[i].modules[k].moduleGrade = parseFloat(this.users[i].modules[k].moduleGrade.toFixed(2));

          const moduleOpenDate = new Date(this.users[i].modules[k].openDate);
          const moduleValuationDate = new Date(this.users[i].modules[k].valuationDate);

          if (moduleOpenDate > today) {
            this.users[i].modules[k].moduleStatus = 1;
          } else {
            if (moduleOpenDate < today && moduleValuationDate > today) {
              this.users[i].modules[k].moduleStatus = 2;
            } else {
              this.users[i].modules[k].moduleStatus = 3;
            }
          }

          if (this.users[i].modules[k].progress  === 1) {
            this.users[i].modules[k].userModuleStatus = 3;
          } else {
            if (this.users[i].modules[k].progress > 0 && this.users[i].modules[k].progress < 1) {
              this.users[i].modules[k].userModuleStatus = 2;
            } else {
              this.users[i].modules[k].userModuleStatus = 1;
            }
          }

        }

        for (let k = 0; k < this.users[i].events.length; k++) {
          this.users[i].events[k].finalGrade = parseFloat(this.users[i].events[k].finalGrade.toFixed(2));
          this.users[i].events[k].date = this.users[i].events[k].date ? new Date(this.users[i].events[k].date) : null;
        }
      }

      console.log('this.users -> ', this.users);

    }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde'));
  }

  public getStatusImage(status: number, state: number): string {

    if (status < state) {
      return './assets/img/approved-disabled.png';
    }

    return './assets/img/approved.png';
  }

  public getEventStatusImage(date: string): string {

    if (date) {
      return './assets/img/approved.png';
    }

    return './assets/img/approved-disabled.png';
  }

  public getPercentage(value: number): number {
    // return parseFloat((value * 100 / this.results.itemsCount).toFixed(2));
    return parseFloat((value * 100 / 6).toFixed(2));
  }
}
