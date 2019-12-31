import { Component, Input, EventEmitter, Output } from '@angular/core';
import { TrackOverview, LateStudent } from 'src/app/models/track-overview.interface';
import { Router } from '@angular/router';
import { User } from 'src/app/models/user.model';
import { TrackPreview } from 'src/app/models/previews/track.interface';
import { SettingsTracksService } from 'src/app/settings/_services/tracks.service';
import { ExcelService } from 'src/app/shared/services/excel.service';
import { MatSnackBar } from '@angular/material';
import { NotificationClass } from 'src/app/shared/classes/notification';
import { AuthService } from 'src/app/shared/services/auth.service';
import { EmptyOutletComponent } from '@angular/router/src/components/empty_outlet';

@Component({
  selector: 'app-track-overview-students',
  template: `
    <div class="students" >
      <p class="title" *ngIf="track && itemsCount > 0">
        <span class="title-total"> Total: {{track.studentsCount}}</span>
        <span class="title-late"> Atrasados: {{getLateStudents(track)}}</span>
      </p>
      <p class="title space" >
        ALUNOS
        <button *ngIf="canExport()"
          class="btn-test"
          (click)="exportStudents()" >
          Exportar Alunos
        </button>
      </p>
      <div class="search" >
        <i class="search-icon icon-lupa"></i>
        <input type="text"
          placeholder="Procure pelo nome"
          (keyup)="searchStudent.emit($event.target.value)"
        />
      </div>
      <p class="no-students" *ngIf="track && itemsCount === 0" >
        Ainda não há alunos inscritos nesta trilha.
      </p>
      <ul *ngIf="track && itemsCount > 0" >
        <li *ngFor="let item of track?.students; let index = index"
          (click)="viewStudentOverview(item)" >
          <img class="photo"
            [src]="item.imageUrl || './assets/img/user-image-placeholder.png'"
          />
          <img class="icon"
              [src]="item.isBlocked ? './assets/img/lock-user-red.png' : './assets/img/unlock-user-green.png'"
            />
          <p class="item-title" >
            {{ item.name }}
          </p>
          <ng-container *ngIf="checkStudentLate(track, item.id) !== null">
            <div class="late" >
              <div>
                atrasado
                <img class="warn" src="./assets/img/status-warning.png" />
              </div>
              <p>empenho: {{ checkStudentLate(track, item.id) * 100 }}%</p>
            </div>
          </ng-container>
          <i class="icon-seta_esquerda"></i>
        </li>
      </ul>

      <app-pagination
        [hidden]="itemsCount === 0"
        [setPagination]="{
          'itemsCount': itemsCount,
          'pageSize': 10
        }"
        (goToPage)="goToPage.emit($event)"
      ></app-pagination>
    </div>`,
  styleUrls: ['./track-students.component.scss']
})
export class TrackOverviewStudentsComponent extends NotificationClass {

  @Input() readonly track: TrackOverview;
  @Input() readonly itemsCount: number = 0;
  @Output() goToPage: EventEmitter<number> = new EventEmitter();
  @Output() searchStudent: EventEmitter<string> = new EventEmitter();
  public isAdmin: boolean = false;

  constructor(
    protected _snackBar: MatSnackBar,
    private _router: Router,
    private _excelService: ExcelService,
    private _tracksService: SettingsTracksService,
    private _authService: AuthService
  ) {
    super(_snackBar);
    this.isAdmin = this._authService.isAdmin();
  }


  public getLateStudents(track: TrackOverview): number {
    const lateStudents: Array<LateStudent> = [];

    if (track.lateStudents) {
      track.lateStudents.forEach(student => {
        if (!lateStudents.some(s => student.studentId === s.studentId))
          lateStudents.push(student);
      });
    }

    return lateStudents.length;
  }

  public checkStudentLate(track: TrackOverview, studentId: string): number | null {
    if (track.lateStudents) {
      const student = track.lateStudents.find(x => x.studentId === studentId);
      if (student && student.lateEvents && student.lateEvents.length > 0)
        return student.progress / student.lateEvents.length;

      return null;
    }
    return null;
  }

  public viewStudentOverview(student: User) {
    this._router.navigate([
      'configuracoes/trilha-de-curso/' + this.track.id + '/' + student.id
    ]);
  }

  public canExport(): boolean {
    const trackPreview = localStorage.getItem('track-responsible');
    if (trackPreview) {
      const track: TrackPreview = JSON.parse(trackPreview);
      if (track && track.subordinate) {
        return !track.subordinate;
      }
    }
    return true;
  }

  public exportStudents() {
    this._tracksService.getTrackOverviewStudents(this.track.id).subscribe(res => {
      this._excelService.exportAsExcelFile(res.data, 'Alunos-Trilha-' + this.track.id);
    }, err => { this.notify(this.getErrorNotification(err)); });
  }
}
