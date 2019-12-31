import { ComponentFixture, async, TestBed } from '@angular/core/testing';
import { AcademicEducationComponent } from '../../manage-user-career/academic-education/academic-education.component';
import { BrowserModule } from '@angular/platform-browser';
import { MaterialComponentsModule } from 'src/app/shared/material.module';
import { FormsModule, ReactiveFormsModule, FormGroup, FormArray, FormControl } from '@angular/forms';
import { ListSearchModule } from 'src/app/shared/components/list-search/list-search.module';
import { SettingsUsersService } from 'src/app/settings/_services/users.service';
import { BackendService, BaseUrlService } from '@tg4/http-infrastructure/dist/src';
import { HttpClientModule } from '@angular/common/http';
import { of } from 'rxjs';
import { careerInstitutesMock } from '../mocks';

const careerInstitutes = careerInstitutesMock;

describe('[Integration] AcademicEducationComponent', () => {

    let fixture: ComponentFixture<AcademicEducationComponent>;
    let component: AcademicEducationComponent;
    let service: SettingsUsersService;

    beforeEach(async(() => {
        TestBed.configureTestingModule({
          imports: [
            BrowserModule,
            MaterialComponentsModule,
            FormsModule,
            ReactiveFormsModule,
            HttpClientModule,
            ListSearchModule
          ],
          declarations: [
            AcademicEducationComponent
          ],
          providers: [
            BackendService,
            BaseUrlService,
            SettingsUsersService
          ]
        }).compileComponents().then(() => {
          fixture = TestBed.createComponent(AcademicEducationComponent);
          component = fixture.componentInstance;
          service = TestBed.get(SettingsUsersService);

          component.formGroup = new FormGroup({
            'colleges': new FormArray([
              new FormGroup({
                'instituteId': new FormControl('5c5c9d0678a3262c08bc7073'),
                'title': new FormControl('Ph'),
                'campus': new FormControl(''),
                'name': new FormControl(''),
                'academicDegree': new FormControl(''),
                'status': new FormControl(''),
                'completePeriod': new FormControl(''),
                'startDate': new FormControl(''),
                'endDate': new FormControl(''),
                'cr': new FormControl('')
              })
            ])
          });
        });
    }));

    it('should emit addEducation event on add education button click', () => {
        const emitSpy = spyOn(component.addEducation, 'emit');

        const button = fixture.nativeElement.querySelector('button.add-button');
        button.dispatchEvent(new Event('click'));
        fixture.detectChanges();

        expect(emitSpy).toHaveBeenCalled();
    });

    it('should load institutes correctly when app-list-search is triggered', () => {
      const getUserInstitutionsSpy = spyOn(
        service, 'getUserInstitutions'
      ).and.callFake(() => of({ data: careerInstitutes }));
      component.institutes.push([]);

      const formArray = component.formGroup.get('colleges') as FormArray;
      const formGroup = formArray.controls[0] as FormGroup;

      component.triggerCollegeSearch('', formGroup, 0);
      fixture.detectChanges();

      expect(getUserInstitutionsSpy).not.toHaveBeenCalled();
      expect(component.institutes[0].length).toBe(0);

      component.triggerCollegeSearch('Ph', formGroup, 0);
      fixture.detectChanges();

      expect(getUserInstitutionsSpy).toHaveBeenCalledWith('Ph');
      expect(component.institutes[0].length).toBe(1);
    });

    it('should fill education correctly', () => {
        fixture.detectChanges();
        setFieldValue('div.campus input', 'Nome do campus/bairro');
        setFieldValue('div.name input', 'Nome do curso');
        setFieldValue('div.startDate input', '01/2000');
        setFieldValue('div.endDate input', '01/2005');
        setFieldValue('div.cr input', '5');

        fixture.detectChanges();
        const result = component.formGroup.getRawValue();
        expect(result.colleges).toBeDefined();
        expect(result.colleges.length).toBe(1);
        expect(result.colleges[0].instituteId).toBe('5c5c9d0678a3262c08bc7073');
        expect(result.colleges[0].title).toBe('Ph');
        expect(result.colleges[0].campus).toBe('Nome do campus/bairro');
        expect(result.colleges[0].name).toBe('Nome do curso');
        expect(result.colleges[0].startDate).toEqual('01/2000');
        expect(result.colleges[0].endDate).toEqual('01/2005');
        expect(result.colleges[0].cr).toBe('5');
    });

    function setFieldValue(query: string, value: string) {
        const companyNameField = fixture.nativeElement.querySelector( query );
        companyNameField.value = value;
        companyNameField.dispatchEvent( new Event('input') );
    }

    function setOptionValue(query: string, value: string) {
      const companyNameField = fixture.nativeElement.querySelector( query );
      companyNameField.value = value;
      companyNameField.dispatchEvent( new Event('mat-select') );
    }
});
