import { Component, ViewChild, OnInit } from '@angular/core';
import { MatStepper, MatDialog, MatSnackBar } from '@angular/material';
import { Router } from '@angular/router';
import { Module, ModuleWeights } from '../../../models/module.model';
import { SupportMaterial } from '../../../models/support-material.interface';
import { Requirement } from './models/new-requirement.model';
import { Subject } from '../../../models/subject.model';
import { Content } from '../../../models/content.model';
import { Question } from '../../../models/question.model';
import { CreatedModuleDialogComponent } from './steps/8_created/created-module.dialog';
import { NewModuleModuleInfoComponent } from './steps/1_module-info/module-info.component';
import { NewModuleSupportMaterialsComponent } from './steps/3_support-materials/support-materials.component';
import { NewModuleRequirementsComponent } from './steps/4_requirements/requirements.component';
import { NewModuleSubjectsComponent } from './steps/5_subjects/subjects.component';
import { NewModuleContentsComponent } from './steps/6_contents/contents.component';
import { NewModuleQuestionsComponent } from './steps/7_questions/questions.component';
import { NotificationClass } from '../../../shared/classes/notification';
import { UtilService } from '../../../shared/services/util.service';
import { NewModuleVideoComponent } from './steps/2_video/video.component';
import { SharedService } from '../../../shared/services/shared.service';
import { Level } from '../../../models/shared/level.interface';
import { ConfirmDialogComponent } from 'src/app/shared/dialogs/confirm/confirm.dialog';
import { SettingsModulesDraftsService } from '../../_services/modules-drafts.service';
import { ModuleGradeTypeEnum } from 'src/app/models/enums/ModuleGradeTypeEnum';
import { ModuleEcommerceComponent } from './steps/9_ecommerce/ecommerce.component';
import { EcommerceProduct } from 'src/app/models/ecommerce-product.model';
import { SettingsTracksService } from '../../_services/tracks.service';
import { SettingsModulesService } from '../../_services/modules.service';
import { NewModulesWeightComponent } from './steps/4.5_weight/modules-weight.component';

@Component({
  selector: 'app-settings-new-module',
  templateUrl: './new-module.component.html',
  styleUrls: ['./new-module.component.scss']
})
export class SettingsNewModuleComponent extends NotificationClass implements OnInit {

  @ViewChild('stepper') stepper: MatStepper;

  @ViewChild('moduleInfo') moduleInfo: NewModuleModuleInfoComponent;
  @ViewChild('moduleVideo') moduleVideo: NewModuleVideoComponent;
  @ViewChild('moduleMaterials') moduleMaterials: NewModuleSupportMaterialsComponent;
  @ViewChild('moduleRequirements') moduleRequirements: NewModuleRequirementsComponent;
  @ViewChild('moduleWeight') moduleWheight: NewModulesWeightComponent;
  @ViewChild('moduleSubjects') moduleSubjects: NewModuleSubjectsComponent;
  @ViewChild('moduleContents') moduleContents: NewModuleContentsComponent;
  @ViewChild('moduleQuestions') moduleQuestions: NewModuleQuestionsComponent;
  @ViewChild('ecommerce') ecommerce: ModuleEcommerceComponent;

  public levels: Array<Level> = [];
  public newModule = new Module();
  public stepIndex: number = 0;
  public loading: boolean = false;
  public allowEditing: boolean = false;
  public showDraftOptions: boolean = false;
  public totalWeight: number = 0;

  private _shouldFinish: boolean = true;
  public moduleWeights: ModuleWeights [];

  constructor(
    protected _snackBar: MatSnackBar,
    private _router: Router,
    private _dialog: MatDialog,
    private _draftsService: SettingsModulesDraftsService,
    private _moduleService: SettingsModulesService,
    private _utilService: UtilService,
    private _sharedService: SharedService
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    this._loadLevels();
    const moduleStr = localStorage.getItem('editingModule');
    if (moduleStr && moduleStr.trim() !== '') {
      this.newModule = new Module( JSON.parse(moduleStr) );
      this.allowEditing = true;
      this.moduleWeights = [
        {
          content: 'AvalDiagnostica',
          // tslint:disable-next-line: max-line-length
          weight: this.newModule.moduleWeights != null && this.newModule.moduleWeights.length > 0 ? this.newModule.moduleWeights.find(x => x.content === 'AvalDiagnostica').weight : 0,
          label: 'AVALIAÇÃO DIAGNÓSITCA'
        },
        {
          content: 'BqdDinamico',
          // tslint:disable-next-line: max-line-length
          weight: this.newModule.moduleWeights != null && this.newModule.moduleWeights.length > 0 ? this.newModule.moduleWeights.find(x => x.content === 'BqdDinamico').weight : 0,
          label: 'BQD DINÁMICO'
        },
        {
          content: 'AvalFinal',
          // tslint:disable-next-line: max-line-length
          weight: this.newModule.moduleWeights != null && this.newModule.moduleWeights.length > 0 ? this.newModule.moduleWeights.find(x => x.content === 'AvalFinal').weight : 0,
          label: 'AVALIAÇÃO FINAL'
        },
      ];
      this.setTotalValue();
    }
  }

  public saveContent(): void {
    this._shouldFinish = this.stepIndex === 8;
    this.nextStep();
  }

  public setTotalValue() {
    this.totalWeight = 0;
    const weights = this.moduleWeights.map(x => x.weight);
    weights.forEach(weight => {
      this.totalWeight += weight ? weight : 0;
    });
  }

  public nextStep(offset: number = 0) {
    switch (this.stepper.selectedIndex + offset) {
      case 0:
        this.moduleInfo.nextStep(); break;
      case 1:
        this.moduleVideo.nextStep(); break;
      case 2:
        this.moduleMaterials.nextStep(); break;
      case 3:
        this.moduleRequirements.nextStep(); break;
      case 4:
        this.moduleWheight.nextStep(); break;
      case 5:
        this.moduleSubjects.nextStep(); break;
      case 6:
        this.moduleContents.nextStep(); break;
      case 7:
        this.ecommerce.nextStep(); break;
      case 8:
        this.moduleQuestions.nextStep(); break;
      default:
        break;
    }
  }

  public previousStep() {
    this.stepIndex--;

    if (this.stepIndex === 6)
      this.moduleContents.ngOnInit();

    this.stepper.previous();
  }

  public stepChanged(event, shouldFinish: boolean = true) {
    setTimeout(() => {
      if (event.previouslySelectedIndex < event.selectedIndex) {
        this._shouldFinish = shouldFinish;
        this.nextStep(-1);
      }
      if (this.stepIndex !== event.selectedIndex)
        this.stepIndex = event.selectedIndex;
    });
  }

  public setModuleInfo(moduleInfo: Module) {
    this.newModule.setModuleInfo( moduleInfo );
    this.newModule.id ?
      this._updateModuleInfo(this.newModule) :
      this._createNewModule(moduleInfo);
  }

  public setModuleVideo(eventVideo: Module) {
    this.newModule.setVideoInfo(eventVideo);
    this._updateModuleInfo( this.newModule );
  }

  public addSupportMaterials(materials: Array<SupportMaterial>) {
    this.newModule.addSupportMaterials( materials );
    this._updateSupportMaterials(this.newModule.id, this.newModule.supportMaterials);
  }

  public setRequirements(requirements: Array<Array<Requirement>>) {
    this.newModule.setRequirements( requirements[0], requirements[1] );
    const requirmentsList = [ ...requirements[0], ...requirements[1] ];
    requirmentsList.forEach((req) => { delete req.module; delete req.editing; });
    this._updateRequirements(this.newModule.id, requirmentsList);
  }

  public addSubjects(subjects: Array<Subject>) {
    this.newModule.addSubjects( subjects );
    this._updateSubjects(this.newModule.id, this.newModule.subjects);
  }

  public addModulesWeights(ModulesWeights: Array<ModuleWeights>) {
    console.log('ModulesWeights -> ', ModulesWeights);
    this.newModule.addModulesWeights( ModulesWeights );
    this._updateModuleWeights(this.newModule.id, this.newModule.moduleWeights);
  }

  public addContents(contents: Array<Content>) {
    this.newModule.addContents( contents );
    if (contents && contents.length > 0) {
      this._updateContents(
        this.newModule.id,
        this.newModule.contents[0].subjectId,
        this.newModule.contents
      );
    } else this._updateFooter();
  }

  public addQuestions(result: { questions: Array<Question>, questionsLimit?: number,
    moduleGradeType: ModuleGradeTypeEnum }) {
    this.newModule.addQuestions( result.questions );
    if (this.newModule.moduleGradeType !== result.moduleGradeType) {
      this.newModule.moduleGradeType = result.moduleGradeType;
      this._updateModuleInfo(this.newModule);
    }

    this.loading = true;
    this._draftsService.setDraftQuestionsLimit(
      this.newModule.id, result.questionsLimit
    ).subscribe(() => {
      this.loading = false;
      this._dialog.open(CreatedModuleDialogComponent);
      this._router.navigate([ 'configuracoes/modulos' ]);

    }, () => this._errorHandlingFunc() );
  }

  public manageEcommerceInfo(ecommerceProducts: Array<EcommerceProduct>) {
    this.loading = true;
    this._moduleService.manageEcommerceProducts(
      this.newModule.id, ecommerceProducts
    ).subscribe(() => {
      if (this._shouldFinish) {
        if (this._dialog.openDialogs.length <= 0) {
          this._dialog.open(CreatedModuleDialogComponent);
          this._router.navigate([ 'configuracoes/modulos' ]);
        }
      }
    });
  }

  public publishDraftChanges(): void {
    const dialogRef = this._dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: { message: 'Tem certeza que deseja publicar as alterações? O módulo será substituído pela versão em rascunho.' }
    });

    dialogRef.afterClosed().subscribe((result: boolean) => {
      if (result) {
        this._draftsService.publishDraft(
          this.newModule.id
        ).subscribe(() => {
          this.notify('Módulo publicado com sucesso!');
          this._router.navigate([ 'configuracoes/modulos' ]);

        }, (error) => this.notify( this.getErrorNotification(error) ));
      }
    });
  }

  public rejectDraftChanges(): void {
    const dialogRef = this._dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: { message: 'Tem certeza que deseja rejeitar as alterações em rascunho? Todas as alterações serão perdidas.' }
    });

    dialogRef.afterClosed().subscribe((result: boolean) => {
      if (result) {
        this._draftsService.rejectDraft(
          this.newModule.id
        ).subscribe(() => {
          this.notify('Alterações rejeitadas com sucesso!');
          this._router.navigate([ 'configuracoes/modulos' ]);

        }, (error) => this.notify( this.getErrorNotification(error) ));
      }
    });
  }

  private _createNewModule(module: Module) {
    this.loading = true;
    this._draftsService.addNewModuleDraft(module).subscribe((response) => {
      this.newModule.id = response.data.id;
      this.newModule.ecommerceId = response.data.ecommerceId;
      this.moduleInfo.setEcommerceId(this.newModule.ecommerceId);

      this._updateFooter();
      this.loading = false;

    }, () => this._errorHandlingFunc() );
  }

  private _updateModuleInfo(module: Module) {
    this.loading = true;
    this._draftsService.updateModuleDraft(module).subscribe(() => {
      this._shouldFinish ?
        this._updateFooter() :
        this._shouldFinish = true;
      this.loading = false;

    }, () => this._errorHandlingFunc() );
  }

  private _updateSupportMaterials(moduleId: string, materials: Array<SupportMaterial>) {
    this.loading = true;
    this._draftsService.manageDraftSupportMaterials(moduleId, materials).subscribe(() => {
      this._shouldFinish ?
        this._updateFooter() :
        this._shouldFinish = true;
      this.loading = false;

    }, () => this._errorHandlingFunc() );
  }

  private _updateRequirements(moduleId: string, requirements: Array<Requirement>) {
    this.loading = true;
    this._draftsService.manageDraftRequirements(moduleId, requirements).subscribe(() => {
      this._shouldFinish ?
        this._updateFooter() :
        this._shouldFinish = true;
      this.loading = false;

    }, () => this._errorHandlingFunc() );
  }

  private _updateSubjects(moduleId: string, subjects: Array<Subject>) {
    this.loading = true;

    if (subjects)
      subjects.forEach((sub) => delete sub.contents);

    this._draftsService.manageDraftSubjects(moduleId, subjects).subscribe((response) => {
      this.newModule.subjects = this._adjustSubjects( response.data );
      this.moduleSubjects.setUserProgess();

      this._shouldFinish ?
        this._updateFooter() :
        this._shouldFinish = true;
      this.loading = false;

    }, () => this._errorHandlingFunc() );
  }


  private _updateModuleWeights(moduleId: string, weights: Array<ModuleWeights>) {
    this.loading = true;

    this._draftsService.manageDraftModuleWeight(moduleId, weights).subscribe(res => {
      this._shouldFinish ?
        this._updateFooter() :
        this._shouldFinish = true;
      this.loading = false;

    }, () => this._errorHandlingFunc() );
  }

  private _updateContents(moduleId: string, subjectId: string, contents: Array<Content>) {
    this.loading = true;
    this._draftsService.manageDraftContents(moduleId, subjectId, contents).subscribe(() => {
      this.moduleQuestions.loadQuestions();
      this._shouldFinish ?
        this._updateFooter() :
        this._shouldFinish = true;
      this.loading = false;

    }, () => this._errorHandlingFunc() );
  }

  private _updateFooter() {
    this.stepIndex++;
    this.stepper.next();
  }

  private _errorHandlingFunc() {
    this.loading = false;
    this.notify('Ocorreu um erro, por favor tente novamente mais tarde');
  }

  private _adjustSubjects(subjects: Array<Subject>): Array<Subject> {
    subjects.forEach(subject => {
      subject.contents.forEach((content: any) => {
        content.subjectId = subject.id;
        content.duration = this._utilService.formatDurationToHour(content.duration);
        content.concepts.forEach(conc => {
          if (conc.positions && conc.positions.length > 0) {
            conc.checked = true;
            conc.positions = conc.positions.map((pos: number) =>
              this._utilService.formatDurationToHour(pos)
            );
          }
        });
      });
    });
    return subjects;
  }

  private _loadLevels(): void {
    this._sharedService.getLevels().subscribe((response) => {
      this.levels = response.data;

    }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde') );
  }

}
