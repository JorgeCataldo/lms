import { Component, AfterViewInit, OnInit } from '@angular/core';
import { NotificationClass } from 'src/app/shared/classes/notification';
import { MatSnackBar } from '@angular/material';
import { SettingsEventsService } from '../../_services/events.service';
import * as Chart from 'chart.js';
import { ActivatedRoute } from '@angular/router';
import { EventResults, Suggestions } from 'src/app/models/previews/event-results.interface';

@Component({
  selector: 'app-settings-event-results',
  templateUrl: './event-results.component.html',
  styleUrls: ['./event-results.component.scss']
})
export class SettingsEventResultsComponent extends NotificationClass implements AfterViewInit {

  public results: EventResults;
  public pieChart;
  public stackedChart;
  public eventId: string = '';
  public scheduleId: string = '';
  private _currentPage: number = 1;

  constructor(
    protected _snackBar: MatSnackBar,
    private _activatedRoute: ActivatedRoute,
    private _eventsService: SettingsEventsService
  ) {
    super(_snackBar);
    this.eventId = this._activatedRoute.snapshot.paramMap.get('eventId');
    this.scheduleId = this._activatedRoute.snapshot.paramMap.get('scheduleId');
  }

  ngAfterViewInit() {
    this._getResults( this.eventId, this.scheduleId );
  }

  public goToPage(page: number) {
    if (page !== this._currentPage) {
      this._currentPage = page;
      this._getResults( this.eventId, this.scheduleId );
    }
  }

  public getPercentage(value: number): number {
    return parseFloat((value * 100 / this.results.itemsCount).toFixed(2));
  }

  public manageSuggestion(suggestions: Suggestions) {
    suggestions.approved = !suggestions.approved;

    this._eventsService.manageEventReaction(
      suggestions.eventReactionId,
      suggestions.approved
    ).subscribe(() => {
      const text = suggestions.approved ? 'aprovado' : 'ocultado';
      this.notify('Comentário ' + text + ' com sucesso!');

    }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde'));
  }

  private _getResults(eventId: string, scheduleId: string): void {
    this._eventsService.getEventReactions(
      eventId, scheduleId, this._currentPage, 5
    ).subscribe((response) => {
      this.results = response.data;

      if (this.results.suggestions) {
        this.results.suggestions = this.results.suggestions.filter(
          sug => sug.suggestion && (sug.suggestion.trim() !== '')
        );
      }

      if (this.results.itemsCount > 0)
        this._plotGraphs();

    }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde'));
  }

  private _plotGraphs(): void {
    this.pieChart = new Chart('pieCanvas', {
      type: 'pie',
      data: {
        labels: [
          'superou a expectativa',
          'dentro da expectativa',
          'abaixo da expectativa'
        ],
        datasets: [{
            data: [
              this.getPercentage( this.results.expectation.exceedExpectation ),
              this.getPercentage( this.results.expectation.asExpected ),
              this.getPercentage( this.results.expectation.belowExpectation )
            ],
            backgroundColor: [
              '#9afa00',
              '#24bcd1',
              '#ff4376'
            ]
          }
        ]
      },
      options: {
        legend: {
          display: false
        },
        tooltips: {
          enabled: false
        },
        scales: {
          xAxes: [{
            display: false
          }],
          yAxes: [{
            display: false
          }],
        }
      }
    });

    this.stackedChart = new Chart('stackedCanvas', {
      type: 'horizontalBar',
      data: {
        labels: [
          'Didática do professor',
          'Conteúdo apresentado em sala',
          'Conteúdo enviado para estudo',
          'Link da teoria com a prática',
          'Recursos utilizados',
          'Forma de avaliação'
        ],
        datasets: [{
          data: [
            this.getPercentage( this.results.didactic.excelent ),
            this.getPercentage( this.results.classroomContent.excelent ),
            this.getPercentage( this.results.studyContent.excelent ),
            this.getPercentage( this.results.theoryAndPractice.excelent ),
            this.getPercentage( this.results.usedResources.excelent ),
            this.getPercentage( this.results.evaluationFormat.excelent ),
          ],
          backgroundColor: '#9afa00'
        }, {
          data: [
            this.getPercentage( this.results.didactic.good ),
            this.getPercentage( this.results.classroomContent.good ),
            this.getPercentage( this.results.studyContent.good ),
            this.getPercentage( this.results.theoryAndPractice.good ),
            this.getPercentage( this.results.usedResources.good ),
            this.getPercentage( this.results.evaluationFormat.good )
          ],
          backgroundColor: '#24bcd1'
        }, {
          data: [
            this.getPercentage( this.results.didactic.satisfactory ),
            this.getPercentage( this.results.classroomContent.satisfactory ),
            this.getPercentage( this.results.studyContent.satisfactory ),
            this.getPercentage( this.results.theoryAndPractice.satisfactory ),
            this.getPercentage( this.results.usedResources.satisfactory ),
            this.getPercentage( this.results.evaluationFormat.satisfactory )
          ],
          backgroundColor: '#bd62ff'
        }, {
          data: [
            this.getPercentage( this.results.didactic.unsatisfactory ),
            this.getPercentage( this.results.classroomContent.unsatisfactory ),
            this.getPercentage( this.results.studyContent.unsatisfactory ),
            this.getPercentage( this.results.theoryAndPractice.unsatisfactory ),
            this.getPercentage( this.results.usedResources.unsatisfactory ),
            this.getPercentage( this.results.evaluationFormat.unsatisfactory )
          ],
          backgroundColor: '#ffa63e'
        }, {
          data: [
            this.getPercentage( this.results.didactic.bad ),
            this.getPercentage( this.results.classroomContent.bad ),
            this.getPercentage( this.results.studyContent.bad ),
            this.getPercentage( this.results.theoryAndPractice.bad ),
            this.getPercentage( this.results.usedResources.bad ),
            this.getPercentage( this.results.evaluationFormat.bad )
          ],
          backgroundColor: '#ff4376'
        }]
      },
      options: {
        legend: {
          display: false
        },
        tooltips: {
          enabled: false
        },
        hover: {
          animationDuration: 0
        },
        animation: {
          onComplete: function () {
            const chartInstance = this.chart;
            const ctx = chartInstance.ctx;
            ctx.textAlign = 'right';
            ctx.font = 'bold 10px';
            ctx.fillStyle = '#3c3c3c';

            Chart.helpers.each(this.data.datasets.forEach((dataset, i) => {
                const meta = chartInstance.controller.getDatasetMeta(i);
                Chart.helpers.each(meta.data.forEach((bar, index) => {
                    const data = dataset.data[index];
                    if (data > 4) {
                      ctx.fillText(
                        data + '%',
                        bar._model.base + 15 + ((bar._model.x - bar._model.base) / 2),
                        bar._model.y + 4
                      );
                    }
                }), this);
            }), this);
          }
        },
        scales: {
          xAxes: [{
            display: false,
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
  }
}
