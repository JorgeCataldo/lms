import { Component, Input, OnInit, Output, EventEmitter } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { MatDialog, MatSnackBar } from '@angular/material';
import { EventApplication } from '../../../models/event-application.model';
import { UserPreview } from '../../../models/previews/user.interface';
import { AnswersDialogComponent } from './answers-dialog/answers.dialog';
import { SettingsEventsService } from '../../_services/events.service';
import { NotificationClass } from 'src/app/shared/classes/notification';

@Component({
  selector: 'app-settings-applications-table',
  templateUrl: './applications-table.component.html',
  styleUrls: ['./applications-table.component.scss']
})
export class SettingsApplicationsTableComponent extends NotificationClass  implements OnInit {

  @Input() header: string = 'APLICAÇÕES PENDENTES';
  @Input() dateLabel: string = 'Solicitado Em';
  @Input() applications: Array<EventApplication> = [];
  @Output() approveApplication = new EventEmitter<string>();
  @Output() denyApplication = new EventEmitter<string>();

  public readonly displayedColumns: string[] = [
    'logo', 'name', 'email', 'category', 'requestDate', 'requirements', 'answers', 'profile', 'actions'
  ];
  public showSearchInput: boolean = false;
  public searchValue: string;
  private _firstLoad: boolean = true;
  public startApplications: Array<EventApplication> = [];
  private _eventId: string;
  private _scheduleId: string;

  constructor(
    protected _snackBar: MatSnackBar,
    private _router: Router,
    private _dialog: MatDialog,
    private _eventsService: SettingsEventsService,
    private _activatedRoute: ActivatedRoute
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    this._eventId = this._activatedRoute.snapshot.paramMap.get('eventId');
    this._scheduleId = this._activatedRoute.snapshot.paramMap.get('scheduleId');
  }

  public startSearch() {
    if (this._firstLoad) {
      this.startApplications = this.applications.slice();
      this._firstLoad = false;
    }
    this.showSearchInput = !this.showSearchInput;
  }

  public filterApplications() {
    this.applications = this.startApplications.filter(ap => {
      return ap.user.name.toLocaleLowerCase().includes(
        this.searchValue.toLowerCase()
      );
    });
  }

  public viewAnswers(application: EventApplication): void {
    this._dialog.open(AnswersDialogComponent, {
      width: '1000px',
      data: application
    });
  }

  public viewUserProfile(user: UserPreview): void {
    this._router.navigate([ '/configuracoes/detalhes-usuario/' + user.id ]);
  }

  public resolveApplication(application: EventApplication, approved: boolean): void {
    this._eventsService.updateEventUserApplicationStatus(this._eventId, this._scheduleId,
      application.user.id, approved ? 1 : 2).subscribe(() => {
        approved ? this.approveApplication.emit(application.user.id) : this.denyApplication.emit(application.user.id);
    }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde'));
  }

  public getCheckImgSrc(check: boolean): string {
    return check ? './assets/img/approved.png' : './assets/img/denied.png';
  }

}
