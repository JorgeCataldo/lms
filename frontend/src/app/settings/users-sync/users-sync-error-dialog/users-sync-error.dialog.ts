import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';
import { ImportError } from 'src/app/models/previews/user-sync.interface';

@Component({
  selector: 'app-users-sync-error-dialog',
  template: `
    <div class="concepts" >
      <table mat-table [dataSource]="importErrors" class="mat-elevation-z8">

        <ng-container matColumnDef="Action">
          <th width="10%" mat-header-cell *matHeaderCellDef> Ação </th>
          <td width="10%" mat-cell *matCellDef="let row">
            {{row.importAction}}
          </td>
        </ng-container>

        <ng-container matColumnDef="Cge">
          <th width="20%" mat-header-cell *matHeaderCellDef> CGE </th>
          <td width="20%" mat-cell *matCellDef="let row">
            {{row.cge}}
          </td>
        </ng-container>

        <ng-container matColumnDef="Name">
          <th width="20%" mat-header-cell *matHeaderCellDef> Nome </th>
          <td width="20%" mat-cell *matCellDef="let row">
            {{row.name}}
          </td>
        </ng-container>

        <ng-container matColumnDef="Username">
          <th width="20%" mat-header-cell *matHeaderCellDef> Usuário </th>
          <td width="20%" mat-cell *matCellDef="let row">
            {{row.username}}
          </td>
        </ng-container>

        <ng-container matColumnDef="Error">
          <th width="30%" mat-header-cell *matHeaderCellDef> Erro </th>
          <td width="30%" mat-cell *matCellDef="let row">
            {{row.importErrorString}}
          </td>
        </ng-container>

        <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
        <tr mat-row *matRowDef="let row; columns: displayedColumns;" ></tr>
      </table>
    </div>
    <p class="dismiss" (click)="dismiss()" >
      fechar
    </p>`,
  styleUrls: ['./users-sync-error.dialog.scss']
})
export class UsersSyncErrorDialogComponent {

  public readonly displayedColumns: string[] = [
    'Action', 'Cge', 'Name', 'Username', 'Error'
  ];

  constructor(
    public dialogRef: MatDialogRef<UsersSyncErrorDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public importErrors: Array<ImportError>
  ) { }

  public dismiss(): void {
    this.dialogRef.close();
  }

}
