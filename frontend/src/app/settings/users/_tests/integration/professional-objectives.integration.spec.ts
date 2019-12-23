import { ComponentFixture, async, TestBed } from '@angular/core/testing';
import { BrowserModule } from '@angular/platform-browser';
import { MaterialComponentsModule } from 'src/app/shared/material.module';
import { FormsModule, ReactiveFormsModule, FormGroup, FormControl } from '@angular/forms';
import { ProfessionalObjectivesComponent } from '../../manage-user-career/professional-objectives/professional-objectives.component';

describe('[Integration] ProfessionalObjectivesComponent', () => {

  let fixture: ComponentFixture<ProfessionalObjectivesComponent>;
  let component: ProfessionalObjectivesComponent;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      imports: [
        BrowserModule,
        MaterialComponentsModule,
        FormsModule,
        ReactiveFormsModule
      ],
      declarations: [
        ProfessionalObjectivesComponent
      ]
    }).compileComponents().then(() => {
      fixture = TestBed.createComponent(ProfessionalObjectivesComponent);
      component = fixture.componentInstance;

      component.formGroup = new FormGroup({
        'shortDateObjectives': new FormControl(''),
        'longDateObjectives': new FormControl('')
      });
    });
  }));

  it('should fill professional objectives correctly', () => {
    fixture.detectChanges();
    setFieldValue('.short textarea', 'Curto Prazo');
    setFieldValue('.long textarea', 'Longo Prazo');
    fixture.detectChanges();

    expect(component.formGroup.valid).toBeTruthy();
    const result = component.formGroup.getRawValue();
    expect(result.shortDateObjectives).toBe('Curto Prazo');
    expect(result.longDateObjectives).toBe('Longo Prazo');
  });

  function setFieldValue(query: string, value: string) {
    const companyNameField = fixture.nativeElement.querySelector( query );
    companyNameField.value = value;
    companyNameField.dispatchEvent( new Event('input') );
  }

});
