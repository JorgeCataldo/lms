import { ComponentFixture, async, TestBed } from '@angular/core/testing';
import { BrowserModule } from '@angular/platform-browser';
import { MaterialComponentsModule } from 'src/app/shared/material.module';
import { RouterTestingModule } from '@angular/router/testing';
import { HttpClientModule } from '@angular/common/http';
import { BackendService, BaseUrlService } from '@tg4/http-infrastructure/dist/src';
import { Router } from '@angular/router';
import { SettingsManageUserCareerComponent } from '../../manage-user-career/manage-user-career.component';
import { SettingsUsersService } from 'src/app/settings/_services/users.service';
import { careerInfoMock } from '../mocks';
import { of } from 'rxjs';
import { FormsModule, ReactiveFormsModule, FormArray } from '@angular/forms';
import { ListSearchModule } from 'src/app/shared/components/list-search/list-search.module';
import { ProfessionalExperienceComponent } from '../../manage-user-career/professional-experience/professional-experience.component';
import { AcademicEducationComponent } from '../../manage-user-career/academic-education/academic-education.component';
import {
  CareerComplementaryExperienceComponent
} from '../../manage-user-career/complementary-experience/complementary-experience.component';
import { CareerComplementaryInfoComponent } from '../../manage-user-career/complementary-info/complementary-info.component';
import { ProfessionalObjectivesComponent } from '../../manage-user-career/professional-objectives/professional-objectives.component';

const careerInfo = careerInfoMock;

describe('[Unit] SettingsManageUserCareerComponent', () => {

  let fixture: ComponentFixture<SettingsManageUserCareerComponent>;
  let component: SettingsManageUserCareerComponent;

  let service: SettingsUsersService;
  let router: Router;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      imports: [
        BrowserModule,
        MaterialComponentsModule,
        RouterTestingModule.withRoutes([]),
        HttpClientModule,
        FormsModule,
        ReactiveFormsModule,
        ListSearchModule
      ],
      declarations: [
        SettingsManageUserCareerComponent,
        ProfessionalExperienceComponent,
        ProfessionalObjectivesComponent,
        AcademicEducationComponent,
        CareerComplementaryExperienceComponent,
        CareerComplementaryInfoComponent
      ],
      providers: [
        BackendService,
        BaseUrlService,
        SettingsUsersService
      ]
    }).compileComponents().then(() => {
      fixture = TestBed.createComponent(SettingsManageUserCareerComponent);
      component = fixture.componentInstance;
      service = TestBed.get(SettingsUsersService);
      router = TestBed.get(Router);
    });
  }));

  it('should call the endpoint to retrieve the user career info from the server', () => {
    const serviceSpy = spyOn(
      service, 'getUserCareer'
    ).and.callFake(() => of({ data: careerInfo }));

    component.ngOnInit();

    expect(serviceSpy).toHaveBeenCalled();
  });

  it('should set travel availability', () => {
    component.selectTravelAvailability(true);
    expect(component.travelAvailability).toBeTruthy();

    component.selectTravelAvailability(false);
    expect(component.travelAvailability).toBeFalsy();
  });

  it('should create the formgroup with the user career info', () => {
    // TODO
  });

  it('should add a new professional experience object to the formArray', () => {
    component.formGroup = component['_createFormGroup'](null);
    component.addProfessionalExperience();

    const experiencesForm = component.formGroup.get('professionalExperiences') as FormArray;
    expect(experiencesForm).toBeDefined();

    const experiences = experiencesForm.getRawValue();
    expect(experiences.length).toBe(1);
    expect(experiences[0].title).toBe('');
    expect(experiences[0].role).toBe('');
    expect(experiences[0].description).toBe('');
    expect(experiences[0].startDate).toBe('');
    expect(experiences[0].endDate).toBe('');
  });

  it('should save career info', () => {
    // TODO
  });

});
