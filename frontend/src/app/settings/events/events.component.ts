import { Component, OnInit } from '@angular/core';
import { MatSnackBar, MatDialog } from '@angular/material';
import { Router } from '@angular/router';
import { EventPreview } from '../../models/previews/event.interface';
import { NotificationClass } from '../../shared/classes/notification';
import { SettingsEventsService } from '../_services/events.service';
import { Subject } from 'rxjs';
import { debounceTime } from 'rxjs/operators';
import { DeleteEventDialogComponent } from './delete-event/delete-event.dialog';
import { SettingsEventsDraftsService } from '../_services/events-drafts.service';

@Component({
  selector: 'app-settings-events',
  templateUrl: './events.component.html',
  styleUrls: ['./events.component.scss']
})
export class SettingsEventsComponent extends NotificationClass implements OnInit {

  public events: Array<EventPreview> = [];
  public eventsCount: number = 0;
  private _eventsPage: number = 1;
  private _searchSubject: Subject<string> = new Subject();

  constructor(
    protected _snackBar: MatSnackBar,
    private _eventsService: SettingsEventsService,
    private _draftsService: SettingsEventsDraftsService,
    private _router: Router,
    private _dialog: MatDialog
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    this._loadEvents( this._eventsPage );
    this._setSearchSubscription();
  }

  public goToPage(page: number) {
    if (page !== this._eventsPage) {
      this._eventsPage = page;
      this._loadEvents(this._eventsPage);
    }
  }

  public updateSearch(searchTextValue: string) {
    this._searchSubject.next( searchTextValue );
  }

  public createNewEvent(): void {
    localStorage.removeItem('editingEvent');
    this._router.navigate([ '/configuracoes/evento' ]);
  }

  public editEvent(event: EventPreview) {
    event.isDraft ?
      this._router.navigate([ '/configuracoes/gerenciar-eventos/' + event.id + '/rascunho' ]) :
      this._router.navigate([ '/configuracoes/gerenciar-eventos/' + event.id ]);
  }

  public deleteEvent(event: EventPreview) {
    const dialogRef = this._dialog.open(DeleteEventDialogComponent, {
      width: '1000px',
      data: event.hasUserProgess ? event.hasUserProgess : false
    });

    dialogRef.afterClosed().subscribe((result: boolean) => {
      if (result) {
        this._eventsService.deleteEventById(event.id).subscribe(() => {
          this.notify('Evento deletado com sucesso');
          const index = this.events.findIndex(x => x.id === event.id);
          this.events.splice(index, 1);
        }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde'));
      }
    });
  }

  private _loadEvents(page: number, searchValue: string = ''): void {
    this._draftsService.getPagedEventsAndDrafts(
      page, 20, searchValue
    ).subscribe((response) => {
      this.events = response.data.events;
      this.eventsCount = response.data.itemsCount;

    }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde'));
  }

  private _setSearchSubscription(): void {
    this._searchSubject.pipe(
      debounceTime(500)
    ).subscribe((searchValue: string) => {
      this._loadEvents(this._eventsPage, searchValue);
    });
  }
}
