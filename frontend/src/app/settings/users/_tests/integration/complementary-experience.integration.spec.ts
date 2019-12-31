// tslint:disable-next-line:max-line-length
import { CareerComplementaryExperienceComponent } from '../../manage-user-career/complementary-experience/complementary-experience.component';
import { ComponentFixture, TestBed, async } from '@angular/core/testing';
import { BrowserModule } from '@angular/platform-browser';
import { MaterialComponentsModule } from 'src/app/shared/material.module';
import { FormsModule, ReactiveFormsModule, FormGroup, FormArray, FormControl } from '@angular/forms';

describe('[Integration] CareerComplementaryExperienceComponent', () => {
    let fixture: ComponentFixture<CareerComplementaryExperienceComponent>;
    let component: CareerComplementaryExperienceComponent;

    beforeEach(async(() => {
      TestBed.configureTestingModule({
        imports: [
          BrowserModule,
          MaterialComponentsModule,
          FormsModule,
          ReactiveFormsModule
        ],
        declarations: [
          CareerComplementaryExperienceComponent
        ]
      }).compileComponents().then(() => {
        fixture = TestBed.createComponent(CareerComplementaryExperienceComponent);
        component = fixture.componentInstance;

        component.formGroup = new FormGroup({
          'abilities': new FormArray([
            new FormGroup({
              'name': new FormControl(''),
              'level': new FormControl('')
            })
          ]),
          'fixedAbilities': new FormArray([])
        });
      });
    }));

    it('should emit addAbility event on add ability button click', () => {
      const emitSpy = spyOn(component.addAbility, 'emit');

      const button = fixture.nativeElement.querySelector('button.add-button');
      button.dispatchEvent(new Event('click'));
      fixture.detectChanges();

      expect(emitSpy).toHaveBeenCalled();
    });

    it('should fill professional ability correctly', () => {
      fixture.detectChanges();
      setFieldValue('div.name input', 'Nome da habilidade');

      fixture.detectChanges();
      const result = component.formGroup.getRawValue();
      expect(result.abilities).toBeDefined();
      expect(result.abilities.length).toBe(1);
      expect(result.abilities[0].name).toBe('Nome da habilidade');
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
