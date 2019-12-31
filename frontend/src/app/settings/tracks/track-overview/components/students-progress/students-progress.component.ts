import { Component, Input } from '@angular/core';
import { StudentProgress } from 'src/app/models/track-overview.interface';
import * as JSZip from 'jszip';
// import * as JSZipUtils from 'jszip-utils';

@Component({
  selector: 'app-track-overview-students-progress',
  template: `
    <div class="students" *ngIf="students?.length > 0">
      <p class="title" >
        PROGRESSO ALUNOS
      </p>
      <table>
        <thead>
          <tr>
            <th></th>
            <th>Objetivo</th>
            <th class="align-text">Nível Alcançado</th>
            <th>Download Arquivos
            <a style="cursor: pointer;" (click)="downloadAllFiles()">
              <img src="./assets/img/download_azul.svg" *ngIf="hasDownload" ></a></th>
          </tr>
        </thead>
        <tbody>
          <tr *ngFor="let student of students" >
            <td width="40%" >
              <img [src]="getUserImgUrl(student)" />
              {{ student.name }}
            </td>
            <td width="20%" >
              <p class="status" *ngIf="!student.finished" >
                <app-progress-bar
                  [completedPercentage]="student.objective | asPercentage"
                  [height]="15"
                ></app-progress-bar>
                {{ student.objective | asPercentage }}%
              </p>
              <p class="finished" *ngIf="student.finished" >
                Finalizado
                <img src="./assets/img/status-success.png" />
              </p>
            </td>
            <td width="20%" >
              <p class="level"
                [ngClass]="{
                  'none': student.level === null || student.level === 0,
                  'beginner': student.level === 1 && student.objective !== 0,
                  'intermediate': student.level === 2,
                  'advanced': student.level === 3,
                  'expert': student.level === 4
                }" >
                <img [src]="getBadgeImgByLevel(student)" />
                {{
                  student.level === null || student.level === 0 ?
                    'Sem Badge' :
                    levelDictionary[student.level]
                }}
              </p>
            </td>
            <td width="20%" >
            <a  href="{{student.userFiles.downloadLink}}" target="_blank" download *ngIf="student.userFiles">
              <img class="approved btn-download" *ngIf="hasDownload" src="./assets/img/download_azul.svg" />
            </a>
            </td>
          </tr>
        </tbody>
      </table>
    </div>`,
  styleUrls: ['./students-progress.component.scss']
})
export class TrackOverviewStudentsProgressComponent {

  @Input() readonly students: Array<StudentProgress> = [];
  @Input() readonly moduleName: string;
  public hasDownload = true;

  public levelDictionary = {
    0: 'SEM BADGE',
    1: 'INICIANTE',
    2: 'INTERMEDIÁRIO',
    3: 'AVANÇADO',
    4: 'EXPERT'
  };

  public getUserImgUrl(student: StudentProgress): string {
    return student && student.imageUrl && student.imageUrl.trim() !== '' ?
      student.imageUrl : './assets/img/user-image-placeholder.png';
  }

  public getBadgeImgByLevel(student: StudentProgress): string {
    if (student.level === null && student.objective === 0)
      return './assets/img/empty-badge.png';

    switch (student.level) {
      case 1:
        return './assets/img/pencil-icon-shadow.png';
      case 2:
        return './assets/img/glasses-icon-shadow.png';
      case 3:
        return './assets/img/brain-icon-shadow.png';
      case 4:
        return './assets/img/brain-dark-icon-shadow.png';
      default:
        return './assets/img/empty-badge.png';
    }
  }

  // public downloadAllFiles(): void {

  //   const urls = [];

  //   this.students.forEach(function(student) {

  //     if (student.userFiles) {
  //       urls.push(student.userFiles.downloadLink);
  //     }
  //   });

  //   const zip = new JSZip();
  //   let count = 0;
  //   const name = this.moduleName + '.zip';
  //   urls.forEach(function(url) {
  //     JSZipUtils.getBinaryContent(url, function (err, data) {
  //       if (err) {
  //         this.notify('Ocorreu um erro, por favor tente novamente mais tarde');
  //         throw err;
  //       }
  //       let split = url.split('/');
  //       split = split[split.length - 1].split('.');

  //       const file = split[split.length - 2] + '.' + split[split.length - 1];
  //       zip.file(file, data,  {binary: true});
  //       count++;
  //       if (count === urls.length) {
  //         zip.generateAsync({type: 'blob'}).then(function(content) {
  //           saveAs(content, name);
  //         });
  //       }
  //     });
  //   });
  // }
}
