import { of } from 'rxjs';
import { ComponentFixture, async, TestBed } from '@angular/core/testing';
import { BrowserModule } from '@angular/platform-browser';
import { MaterialComponentsModule } from 'src/app/shared/material.module';
import { RouterTestingModule } from '@angular/router/testing';
import { HttpClientModule } from '@angular/common/http';
import { BackendService, BaseUrlService } from '@tg4/http-infrastructure/dist/src';
import { Router, ActivatedRoute, Params } from '@angular/router';
import { ModuleComponent } from '../../module.component';
import { ContentModulesService } from 'src/app/pages/_services/modules.service';
import { moduleMock } from '../mocks';
import { ModuleHeaderComponent } from '../../module-header/module-header.component';
import { ModuleSubjectComponent } from '../../module-subject/module-subject.component';
import { SubjectContentComponent } from '../../module-subject/subject-content/subject-content.component';
import { RequiredModuleComponent } from '../../required-module/required-module.component';
import { ProgressBarModule } from 'src/app/shared/components/layout/progress-bar/progress-bar.module';
import { ModuleSidebarModule } from 'src/app/shared/components/module-sidebar/module-sidebar.module';
import { PipesModule } from 'src/app/shared/pipes/pipes.module';
import { DirectivesModule } from 'src/app/shared/directives/directives.module';
import { UtilService } from 'src/app/shared/services/util.service';
import { SharedService } from 'src/app/shared/services/shared.service';
import { UploadService } from 'src/app/shared/services/upload.service';
import { UserService } from 'src/app/pages/_services/user.service';
import { ContentForumService } from 'src/app/pages/_services/forum.service';
import { AuthService } from 'src/app/shared/services/auth.service';

const dbModule = moduleMock;

describe('[Unit] ModuleComponent', () => {

  let fixture: ComponentFixture<ModuleComponent>;
  let component: ModuleComponent;

  let service: ContentModulesService;
  let router: Router;
  let activatedRoute: ActivatedRoute;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      imports: [
        BrowserModule,
        MaterialComponentsModule,
        RouterTestingModule.withRoutes([]),
        HttpClientModule,
        ProgressBarModule,
        ModuleSidebarModule,
        PipesModule,
        DirectivesModule
      ],
      declarations: [
        ModuleComponent,
        ModuleHeaderComponent,
        ModuleSubjectComponent,
        SubjectContentComponent,
        RequiredModuleComponent
      ],
      providers: [
        BackendService,
        BaseUrlService,
        UtilService,
        SharedService,
        UploadService,
        UserService,
        ContentModulesService,
        ContentForumService,
        AuthService
      ]
    }).compileComponents().then(() => {
      fixture = TestBed.createComponent(ModuleComponent);
      component = fixture.componentInstance;
      service = TestBed.get(ContentModulesService);
      router = TestBed.get(Router);
      activatedRoute = TestBed.get(ActivatedRoute);
    });
  }));

});
