import { ComponentFixture, async, TestBed } from '@angular/core/testing';
import { BrowserModule } from '@angular/platform-browser';
import { MaterialComponentsModule } from 'src/app/shared/material.module';
import { FormsModule, ReactiveFormsModule, FormGroup, FormArray, FormControl } from '@angular/forms';
import { ProfessionalExperienceComponent } from '../../manage-user-career/professional-experience/professional-experience.component';

describe('[Integration] ProfessionalExperienceComponent', () => {

  let fixture: ComponentFixture<ProfessionalExperienceComponent>;
  let component: ProfessionalExperienceComponent;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      imports: [
        BrowserModule,
        MaterialComponentsModule,
        FormsModule,
        ReactiveFormsModule
      ],
      declarations: [
        ProfessionalExperienceComponent
      ]
    }).compileComponents().then(() => {
      fixture = TestBed.createComponent(ProfessionalExperienceComponent);
      component = fixture.componentInstance;

      component.formGroup = new FormGroup({
        'professionalExperience': new FormControl(false),
        'professionalExperiences': new FormArray([
          new FormGroup({
            'title': new FormControl(''),
            'role': new FormControl(''),
            'description': new FormControl(''),
            'startDate': new FormControl(''),
            'endDate': new FormControl('')
          })
        ])
      });
    });
  }));

  it('should toggle the display of professional experience form', () => {
    let button = fixture.nativeElement.querySelector('div.choice-box div div.answer:first-child');
    button.dispatchEvent(new Event('click'));
    fixture.detectChanges();
    let professionalExperience = component.formGroup.getRawValue().professionalExperience;

    let elements = fixture.nativeElement.querySelectorAll('div.prof-experience');
    let addButton = fixture.nativeElement.querySelector('button.add-button');
    expect(professionalExperience).toBeTruthy();
    expect(elements.length).toBe(1);
    expect(addButton.disabled).toBeFalsy();

    button = fixture.nativeElement.querySelector('div.choice-box div div.answer:last-child');
    button.dispatchEvent(new Event('click'));
    fixture.detectChanges();
    professionalExperience = component.formGroup.getRawValue().professionalExperience;

    elements = fixture.nativeElement.querySelectorAll('div.prof-experience');
    addButton = fixture.nativeElement.querySelector('button.add-button');
    expect(professionalExperience).toBeFalsy();
    expect(elements.length).toBe(0);
    expect(addButton.disabled).toBeTruthy();
  });

  it('should emit addExperience event on add experience button click', () => {
    component.formGroup.get('professionalExperience').setValue(true);
    const emitSpy = spyOn(component.addExperience, 'emit');

    const button = fixture.nativeElement.querySelector('button.add-button');
    button.dispatchEvent(new Event('click'));
    fixture.detectChanges();

    expect(emitSpy).toHaveBeenCalled();
  });

  it('should fill professional experience correctly', () => {
    component.formGroup.get('professionalExperience').setValue(true);
    fixture.detectChanges();

    setFieldValue('div.companyName input', 'Nome da Empresa');
    setFieldValue('div.role input', 'Cargo');
    setFieldValue('div.description input', 'Descrição');
    setFieldValue('div.startDate input', '2000-01-01');
    setFieldValue('div.endDate input', '2005-01-01');
    fixture.detectChanges();

    expect(component.formGroup.valid).toBeTruthy();
    const result = component.formGroup.getRawValue();
    expect(result.professionalExperiences).toBeDefined();
    expect(result.professionalExperiences.length).toBe(1);
    expect(result.professionalExperiences[0].title).toBe('Nome da Empresa');
    expect(result.professionalExperiences[0].role).toBe('Cargo');
    expect(result.professionalExperiences[0].description).toBe('Descrição');
    expect(result.professionalExperiences[0].startDate).toEqual( new Date('2000-01-01') );
    expect(result.professionalExperiences[0].endDate).toEqual( new Date('2005-01-01') );
  });

  function setFieldValue(query: string, value: string) {
    const companyNameField = fixture.nativeElement.querySelector( query );
    companyNameField.value = value;
    companyNameField.dispatchEvent( new Event('input') );
  }

});
