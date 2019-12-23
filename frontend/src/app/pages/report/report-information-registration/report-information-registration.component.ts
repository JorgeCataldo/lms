import { Component, Input } from '@angular/core';
import { Requirement } from '../../../settings/modules/new-module/models/new-requirement.model';
import { Level } from '../../../models/shared/level.interface';
import { ExcelService } from 'src/app/shared/services/excel.service';
import { SettingsTracksService } from 'src/app/settings/_services/tracks.service';
import { TestTrack } from 'src/app/models/valuation-test.interface';
import { TrackPreview } from 'src/app/models/previews/track.interface';
import { TrackOverview } from 'src/app/models/track-overview.interface';
import { MatSnackBar } from '@angular/material';
import { NotificationClass } from 'src/app/shared/classes/notification';
import { ReportsService } from 'src/app/settings/_services/reports.service';

@Component({
  selector: 'app-report-information-registration',
  templateUrl: './report-information-registration.component.html',
  styleUrls: ['./report-information-registration.component.scss']
})
export class ReportInformationRegistrationComponent extends NotificationClass {

  @Input() requirement: Requirement;
  @Input() levels: Array<Level>;
  @Input() last: boolean = false;

  public tracks: Array<TestTrack> = [];
  public selectedTracks: Array<TestTrack> = [];
  public getManageInfo: boolean;

  constructor(
    private _excelService: ExcelService,
    private _tracksService: SettingsTracksService,
    private _reportService: ReportsService,
    protected _snackBar: MatSnackBar,
  ) {
    super(_snackBar);
   }

  public triggerTrackSearch(searchValue: string) {
    this._loadTracks(searchValue);
  }

  public removeSelectedTrack(id: string) {
    this._removeFromCollection(
      this.tracks, this.selectedTracks, id
    );
  }

  private _removeFromCollection(collection, selected, id: string): void {
    const selectedTrackIndex = selected.findIndex(x => x.id === id);
    selected.splice(selectedTrackIndex , 1);

    const trackIndex = collection.findIndex(x => x.id === id);
    collection[trackIndex].checked = false;
  }

  private _loadTracks(searchValue: string = ''): void {
    if (searchValue === '') {
      this.tracks = [];
      return;
    }

    this._tracksService.getPagedFilteredTracksList(
      1, 20, searchValue
    ).subscribe(response => {
      response.data.tracks.forEach((tck: TrackPreview) => {
        tck.checked = this.selectedTracks.find(t => t.id === tck.id) && true;
      });
      this.tracks = response.data.tracks;
    });
  }

  public updateTracks(): void {
    this.selectedTracks = this._updateCollection(
      this.tracks, this.selectedTracks
    );
  }

  private _updateCollection(collection, selected): Array<any> {
    const prevSelected = selected.filter(x =>
      !collection.find(t => t.id === x.id)
    );
    const selectedColl = collection.filter(track =>
      track.checked
    );
    return [ ...prevSelected, ...selectedColl];
  }


  public exportRegistrationUser() {
    this.loadTrackReportStudents();
  }

  public exportFinanceReport() {
    this._reportService.getFinanceReport().subscribe(res => {
      const extract = res.data;
      if (extract) {
        const excelModule = [];
          for (let idx = 0; idx < extract.length; idx++) {
            const movement = extract[idx];
            for (let i = 0; i < movement.payables.length; i++) {
              const payable = movement.payables[i];

              excelModule.push({
                'Id da Transação': movement.id,
                'Status da Transação': movement.status,
                'Data da Criação': movement.formated_date_created,
                'Data da Última Atualização': movement.formated_date_updated,
                'Valor Pago': Number(movement.paid_amount / 100),
                'Parcelas': movement.installments,
                'Método de Pagamento': movement.payment_method,
                'Estado': movement.address ? movement.address.state : '',
                'Produto': movement.product,
                'Número do Pedido': movement.metadata ? movement.metadata.order_number : '',
                'Número do Documento': movement.customer ? movement.customer.document_number : '',
                'Tipo do Documento': movement.customer ? movement.customer.document_type : '',
                'Nome': movement.customer ? movement.customer.name : '',
                'Email': movement.customer ? movement.customer.email : '',
                'Status do Recebível': payable.status,
                'Tipo do Recebível': payable.type,
                'Valor do Recebível': Number(payable.amount / 100),
                'Taxa': Number(payable.fee / 100),
                'Taxa de Antecipação': Number(payable.anticipation_fee / 100),
                'Parcela': payable.installment,
                'Dia do Pagamento': payable.formated_payment_date,
                'Dia da Competência': payable.formated_accrual_date,
                'Valor Líquido': Number((payable.amount - payable.fee) / 100)
              });

            }
          }
          this._excelService.exportAsExcelFile(excelModule, 'Relatório Financeiro');
      }
    }, () => {
      console.log('Erro ao exportar relatorio financeiro.');
    });
  }

  public loadTrackReportStudents() {
    this._tracksService.getTrackReportStudents().subscribe(res => {
      const users = res.data;
      if (users) {
        const excelModule = [];

        for (let idx = 0; idx < users.length; idx++) {
          const user = users[idx];

          excelModule.push({
            'Nome': user.name,
            'CPF': user.cpf,
            'Companhia': user.company ? user.company.name : null,
            'Estado': user.address ? user.address.state : null,
            'Cidade': user.address ? user.address.city : null,
            'Responsável': user.responsible,
            'Grupo': user.businessGroup ? user.businessGroup.name : null,
            'Unidade': user.businessUnit ? user.businessUnit.name : null,
            'Segmento': user.segment ? user.segment.name : null,
            'Id de registro': user.registrationId,
            'Nome de usuário': user.userName,
            'E-mail': user.email,
            'Telefone': user.phone,
            'Bloqueado': user.isBlocked
          });
        }

        this._excelService.exportAsExcelFile(excelModule, 'Perfil-Cadastral-Usuários');
      }
    }, () => {
      console.log('Erro ao exportar relatorio de cadastro de usuários.');
    });
  }

  private _getLevelName(level: number): string {
    switch (level) {
      case 1:
        return 'Iniciante';
      case 2:
        return 'Intermediário';
      case 3:
        return 'Avançado';
      case 4:
        return 'Expert';
      default:
        return 'Sem Badge';
    }
  }

}
