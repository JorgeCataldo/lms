import { Component, Inject, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';
import Player from '@vimeo/player';
import { Router } from '@angular/router';

@Component({
  selector: 'app-job-recommendation-dialog',
  template: `
  <ng-container *ngIf="data.jobs === null || data.jobs.length === 0">
    <div class="new-job">
      <p><b>Você não tem vagas</b></p>
      <p>Abra uma vaga e encontre os melhores talentos para sua empresa</p>
      <button class="btn-test" (click)="newJob()">
        Nova Vaga
      </button>
    </div>
  </ng-container>
  <ng-container *ngIf="data.jobs !== null && data.jobs.length > 0">
    <table mat-table [dataSource]="data.jobs">
      <ng-container matColumnDef="name">
          <th width="30%" mat-header-cell *matHeaderCellDef> Nome Vaga </th>
          <td width="30%" mat-cell *matCellDef="let row">
              {{ row.title }}
          </td>
      </ng-container>

      <ng-container matColumnDef="date_due">
          <th width="20%" mat-header-cell *matHeaderCellDef> Prazo de Conclusão </th>
          <td width="20%" mat-cell *matCellDef="let row">
              {{ row.dueTo | date:'dd/MM/yyy' }}
          </td>
      </ng-container>

      <ng-container matColumnDef="candidate_num">
          <th width="30%" mat-header-cell *matHeaderCellDef> Nº de Candidatos </th>
          <td width="30%" mat-cell *matCellDef="let row">
              {{ row.candidatesCount ? row.candidatesCount : '0' }} Candidato(s)
          </td>
      </ng-container>

      <ng-container matColumnDef="arrow">
          <th width="10%" mat-header-cell *matHeaderCellDef></th>
          <td width="10%" mat-cell *matCellDef="let row" >
              <i class="icon-seta_esquerda" style="color: rgba(0, 0, 0, 0.54);"></i>
          </td>
      </ng-container>

      <tr mat-header-row *matHeaderRowDef="openDisplayedColumns"></tr>
      <tr mat-row *matRowDef="let row; columns: openDisplayedColumns;"
          (click)="SelectJob(row)"
      ></tr>
    </table>
    <div class="new-job">
    <button class="btn-test" (click)="newJob()">
        Nova Vaga
    </button>
    </div>
  </ng-container>
  `,
  styleUrls: ['./job-recommendation.dialog.scss']
})
export class JobRecommendationDialogComponent {

  public readonly openDisplayedColumns: string[] = [
    'name', 'date_due', 'candidate_num', 'arrow'
  ];

  constructor(
    private _router: Router,
    public dialogRef: MatDialogRef<JobRecommendationDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { jobs: any[] }) {}


  public SelectJob(job: any): void {
    this.dialogRef.close(job.id);
  }

  public newJob() {
    this.dialogRef.close();
    this._router.navigate(['configuracoes/cadastro-vaga/0']);
  }

}
