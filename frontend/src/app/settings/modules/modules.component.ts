import { Component, OnInit } from '@angular/core';
import { ContentModulesService } from '../../pages/_services/modules.service';
import { MatSnackBar, MatDialog } from '@angular/material';
import { NotificationClass } from '../../shared/classes/notification';
import { Module } from '../../models/module.model';
import { ModulePreview } from '../../models/previews/module.interface';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { debounceTime } from 'rxjs/operators';
import { ExcelService } from 'src/app/shared/services/excel.service';
import { ModuleExcel } from 'src/app/models/module-excel.model';
import { AuthService } from 'src/app/shared/services/auth.service';
import { DeleteModuleDialogComponent } from './delete-module/delete-module.dialog';
import { CloneModuleDialogComponent } from './clone-module/clone-module.dialog';
import { SettingsModulesDraftsService } from '../_services/modules-drafts.service';

@Component({
  selector: 'app-settings-modules',
  templateUrl: './modules.component.html',
  styleUrls: ['./modules.component.scss']
})
export class SettingsModulesComponent extends NotificationClass implements OnInit {

  public modules: Array<Module> = [];
  public modulesCount: number = 0;
  private _modulesPage: number = 1;
  private _searchSubject: Subject<string> = new Subject();
  public user: any;

  constructor(
    protected _snackBar: MatSnackBar,
    private _modulesService: ContentModulesService,
    private _modulesDraftsService: SettingsModulesDraftsService,
    private _excelService: ExcelService,
    private _authService: AuthService,
    private _router: Router,
    private _dialog: MatDialog
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    this._loadModules(this._modulesPage);
    this._setSearchSubscription();
    this.user = this._authService.getLoggedUser();
  }

  public goToPage(page: number) {
    if (page !== this._modulesPage) {
      this._modulesPage = page;
      this._loadModules(this._modulesPage);
    }
  }

  public exportModules() {
    const excelModule: ModuleExcel[] = [];
    for (let moduleIndex = 0; moduleIndex < this.modules.length; moduleIndex++) {
      const module = this.modules[moduleIndex];
      if (module.subjects === null || module.subjects.length === 0) {
        excelModule.push({moduleId: module.id, moduleName: module.title});
      } else {
        for (let subjectIndex = 0; subjectIndex < module.subjects.length; subjectIndex++) {
          const subject = module.subjects[subjectIndex];
          excelModule.push({moduleId: module.id, moduleName: module.title, subjectId: subject.id, subjectName: subject.title});
        }
      }
    }
    this._excelService.exportAsExcelFile(excelModule, 'Modulos');
  }

  public updateSearch(searchTextValue: string) {
    this._searchSubject.next( searchTextValue );
  }

  public createNewModule(): void {
    localStorage.removeItem('editingModule');
    this._router.navigate([ '/configuracoes/modulo' ]);
  }

  public editModule(module: ModulePreview) {
    if (module.isDraft) {
      this._modulesDraftsService.getDraftById(module.id).subscribe((response) => {
        this._goToEditModule(response);

      }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde'));

    }  else {
      this._modulesService.getModuleById(module.id).subscribe((response) => {
        this._goToEditModule(response);

      }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde'));
    }
  }

  public cloneModule(module: ModulePreview) {

    const dialogRef = this._dialog.open(CloneModuleDialogComponent, {
      width: '1000px'
    });

    dialogRef.afterClosed().subscribe((result: boolean) => {

      if (result) {
        let newModule;

    const cloneMethod = module.isDraft ? this._modulesDraftsService.getDraftById(module.id)
    : this._modulesService.getModuleById(module.id);

    cloneMethod.subscribe((response) => {
      newModule = response.data;
      newModule.moduleId = module.isDraft ? module.moduleId : module.id;
      newModule.id = null;
      newModule.title = newModule.title + ' - copy';
      this._modulesDraftsService.cloneNewModuleDraft(newModule).subscribe((newModuleResponse) => {
        newModuleResponse.data.isDraft = true;

        this.modules.push(newModuleResponse.data);
        this.notify('Módulo duplicado com sucesso.');
      });
    }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde'));
      }
    });
  }

  private _goToEditModule(response): void {
    localStorage.setItem('editingModule', JSON.stringify(response.data));
    this._router.navigate([ '/configuracoes/modulo' ]);
  }

  public deleteModule(module: ModulePreview): void {
    const dialogRef = this._dialog.open(DeleteModuleDialogComponent, {
      width: '1000px',
      data: module.hasUserProgess
    });

    dialogRef.afterClosed().subscribe((result: boolean) => {
      if (result) {
        const moduleId = module.isDraft ? module.moduleId : module.id;
        this._modulesService.deleteModuleById(moduleId).subscribe(() => {
          this.notify('Módulo deletado com sucesso');
          this._loadModules(this._modulesPage);
        }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde'));
      }
    });
  }

  private _loadModules(page: number, searchValue: string = ''): void {
    this._modulesDraftsService.getPagedModulesAndDrafts(
      page, 20, searchValue
    ).subscribe((response) => {
      this.modules = response.data.modules;
      this.modulesCount = response.data.itemsCount;

    }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde'));
  }

  private _setSearchSubscription(): void {
    this._searchSubject.pipe(
      debounceTime(500)
    ).subscribe((searchValue: string) => {
      this._loadModules(this._modulesPage, searchValue);
    });
  }

}
