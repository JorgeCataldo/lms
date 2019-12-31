import { Component, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material';
import { NotificationClass } from '../../shared/classes/notification';
import { EventValuation, EventValuationDescription, EventReaction } from 'src/app/models/event-valuation.model';
import { SettingsEventsService } from '../_services/events.service';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-settings-events-valuation',
  templateUrl: './events-valuation.component.html',
  styleUrls: ['./events-valuation.component.scss']
})
export class EventsValuationComponent extends NotificationClass implements OnInit {

  public eventName: string = '';
  public eventDate: Date;
  public eventValuations: EventValuation[] = [];
  public sliderValue: number = 50;
  public eventId: string;
  public _event: any;
  public schedule: any;
  public eventScheduleId: string;
  private _sugestion: string = '';

  constructor(
    protected _snackBar: MatSnackBar,
    private _activatedRoute: ActivatedRoute,
    private _router: Router,
    private _eventsService: SettingsEventsService
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    this.eventValuations = this._createEventValuations();
    this.eventId = this._activatedRoute.snapshot.paramMap.get('eventId');
    this.eventScheduleId = this._activatedRoute.snapshot.paramMap.get('scheduleId');
    localStorage.removeItem('emailUrl');

    this._eventsService.getEventById(this.eventId, 'true').subscribe((response) => {
      this._event = response.data;
      this.schedule = this._event ? this._event.schedules.find(x => x.id === this.eventScheduleId) : null;

      if (!this._event || !this.schedule) {
        this._router.navigate(['']);
        this.notify('Evento não encontrado.');
        return;
      }

      this.eventName = this._event.title;
      this.eventDate = this.schedule.eventDate;

    }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde'));
  }

  public setValue(valuation: EventValuation, value: EventValuationDescription) {
    valuation.values.forEach(element => {
      element.selected = false;
    });
    value.selected = true;
  }

  public getValue(valuation: EventValuation) {
    return valuation.values.find(x => x.selected);
  }

  public getSliderValue() {
    if (this.sliderValue <= 30) {
      return {
        title: 'Ruim em relação ás expectativas',
        value: 0
      };
    } else if (this.sliderValue > 30 && this.sliderValue <= 70) {
      return {
        title: 'Insatisfatório em relação ás expectativas',
        value: 1
      };
    } else {
      return {
        title: 'Satisfatório em relação ás expectativas',
        value: 2
      };
    }
  }

  public setSliderValue(value: number) {
    this.sliderValue = value;
  }

  public setSugestion(event) {
    this._sugestion = event.target.value;
  }

  private _buildReaction(): EventReaction {
    let hasAllValues: boolean = true;
    const reactions: EventValuationDescription[] = [];
    for (let i = 0; i < this.eventValuations.length; i++) {
      const val = this.eventValuations[i].values.find(x => x.selected);
      if (val) {
        reactions.push(val);
      } else {
        hasAllValues = false;
        break;
      }
    }
    if (hasAllValues) {
      return {
        eventId: this.eventId,
        eventScheduleId: this.eventScheduleId,
        didactic: reactions[0].value,
        classroomContent: reactions[1].value,
        studyContent: reactions[2].value,
        theoryAndPractice: reactions[3].value,
        usedResources: reactions[4].value,
        evaluationFormat: reactions[5].value,
        expectation: this.getSliderValue().value,
        suggestions: this._sugestion
      };
    } else {
      this.notify('Por favor avalie todas as categorias.');
      return null;
    }
  }

  public sendValuation() {
    const reaction = this._buildReaction();
    if (reaction) {
      this._eventsService.addEventReaction(reaction).subscribe(() => {
        this.notify('Avaliação enviada com sucesso!');
        this._router.navigate(['']);

      }, (err) => {
        const msg = err.error.errors ? err.error.errors[0] : 'Ocorreu um erro, por favor tente novamente mais tarde';
        this.notify(msg);
      });
    }
  }

  private _createEventValuations(): EventValuation[] {
    return [
      // Didactic
      {
        title: 'Quanto á didática do professor, você avaliaria como:',
        values: [
          {
            title: 'Didática Ruim',
            description: null, // 'Breve Descrição do que seria',
            selected: false,
            value: 0
          },
          {
            title: 'Didática Insatisfatória',
            description: null, // 'Breve Descrição do que seria',,
            selected: false,
            value: 1
          },
          {
            title: 'Didática Satisfatória',
            description: null, // 'Breve Descrição do que seria',,
            selected: false,
            value: 2
          },
          {
            title: 'Boa Didática',
            description: null, // 'Breve Descrição do que seria',,
            selected: false,
            value: 3
          },
          {
            title: 'Excelente Didática',
            description: null, // 'Breve Descrição do que seria',,
            selected: false,
            value: 4
          }
        ]
      },
      // ClassroomContent
      {
        title: 'Quanto ao conteúdo em sala, você avaliaria como:',
        values: [
          {
            title: 'Conteúdo em sala Ruim',
            description: null, // 'Breve Descrição do que seria',,
            selected: false,
            value: 0
          },
          {
            title: 'Conteúdo em sala Insatisfatório',
            description: null, // 'Breve Descrição do que seria',,
            selected: false,
            value: 1
          },
          {
            title: 'Conteúdo em sala Satisfatório',
            description: null, // 'Breve Descrição do que seria',,
            selected: false,
            value: 2
          },
          {
            title: 'Bom Conteúdo em sala',
            description: null, // 'Breve Descrição do que seria',,
            selected: false,
            value: 3
          },
          {
            title: 'Excelente conteúdo em sala',
            description: null, // 'Breve Descrição do que seria',,
            selected: false,
            value: 4
          }
        ]
      },
      // StudyContent
      {
        title: 'Quanto conteúdo de estudo, você avaliaria como:',
        values: [
          {
            title: 'Conteúdo de estudo Ruim',
            description: null, // 'Breve Descrição do que seria',,
            selected: false,
            value: 0
          },
          {
            title: 'Conteúdo de estudo Insatisfatório',
            description: null, // 'Breve Descrição do que seria',,
            selected: false,
            value: 1
          },
          {
            title: 'Conteúdo de estudo Satisfatório',
            description: null, // 'Breve Descrição do que seria',,
            selected: false,
            value: 2
          },
          {
            title: 'Bom conteúdo de estudo',
            description: null, // 'Breve Descrição do que seria',,
            selected: false,
            value: 3
          },
          {
            title: 'Excelente conteúdo de estudo',
            description: null, // 'Breve Descrição do que seria',,
            selected: false,
            value: 4
          }
        ]
      },
      // TheoryAndPractice
      {
        title: 'Quanto á teoria e prática, você avaliaria como:',
        values: [
          {
            title: 'Teoria e prática Ruim',
            description: null, // 'Breve Descrição do que seria',,
            selected: false,
            value: 0
          },
          {
            title: 'Teoria e prática Insatisfatória',
            description: null, // 'Breve Descrição do que seria',,
            selected: false,
            value: 1
          },
          {
            title: 'Teoria e prática Satisfatória',
            description: null, // 'Breve Descrição do que seria',,
            selected: false,
            value: 2
          },
          {
            title: 'Boa teoria e prática',
            description: null, // 'Breve Descrição do que seria',,
            selected: false,
            value: 3
          },
          {
            title: 'Excelente teoria e prática',
            description: null, // 'Breve Descrição do que seria',,
            selected: false,
            value: 4
          }
        ]
      },
      // UsedResources
      {
        title: 'Quanto aos recursos do usuário, você avaliaria como:',
        values: [
          {
            title: 'Recursos ao usuário Ruim',
            description: null, // 'Breve Descrição do que seria',,
            selected: false,
            value: 0
          },
          {
            title: 'Recursos ao usuário Insatisfatório',
            description: null, // 'Breve Descrição do que seria',,
            selected: false,
            value: 1
          },
          {
            title: 'Recursos ao usuário Satisfatório',
            description: null, // 'Breve Descrição do que seria',,
            selected: false,
            value: 2
          },
          {
            title: 'Bom recursos ao usuário',
            description: null, // 'Breve Descrição do que seria',,
            selected: false,
            value: 3
          },
          {
            title: 'Excelente recursos ao usuário',
            description: null, // 'Breve Descrição do que seria',,
            selected: false,
            value: 4
          }
        ]
      },
      // EvaluationFormat
      {
        title: 'Quanto ao formato de avaliação, você avaliaria como:',
        values: [
          {
            title: 'Formato de avaliação Ruim',
            description: null, // 'Breve Descrição do que seria',,
            selected: false,
            value: 0
          },
          {
            title: 'Formato de avaliação Insatisfatório',
            description: null, // 'Breve Descrição do que seria',
            selected: false,
            value: 1
          },
          {
            title: 'Formato de avaliação Satisfatório',
            description: null, // 'Breve Descrição do que seria',
            selected: false,
            value: 2
          },
          {
            title: 'Bom formato de avaliação',
            description: null, // 'Breve Descrição do que seria',
            selected: false,
            value: 3
          },
          {
            title: 'Excelente formato de avaliação',
            description: null, // 'Breve Descrição do que seria',
            selected: false,
            value: 4
          }
        ]
      },
      // Expectations
      {
        title: 'Em relação às suas expectativas, o evento em geral:',
        values: [
          {
            title: 'Ruim em relação ás expectativas',
            description: null, // 'Breve Descrição do que seria',
            selected: false,
            value: 0
          },
          {
            title: 'Insatisfatório em relação ás expectativas',
            description: null, // 'Breve Descrição do que seria',
            selected: false,
            value: 1
          },
          {
            title: 'Satisfatório em relação ás expectativas',
            description: null, // 'Breve Descrição do que seria',
            selected: false,
            value: 2
          },
          {
            title: 'Bom em relação ás expectativas',
            description: null, // 'Breve Descrição do que seria',
            selected: false,
            value: 3
          },
          {
            title: 'Excelente em relação ás expectativas',
            description: null, // 'Breve Descrição do que seria',
            selected: false,
            value: 4
          }
        ]
      }
    ];
  }
}
