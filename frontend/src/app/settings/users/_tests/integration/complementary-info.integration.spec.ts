import { ComponentFixture, async, TestBed } from '@angular/core/testing';
import { CareerComplementaryInfoComponent } from '../../manage-user-career/complementary-info/complementary-info.component';
import { BrowserModule } from '@angular/platform-browser';
import { MaterialComponentsModule } from 'src/app/shared/material.module';
import { FormsModule, ReactiveFormsModule, FormGroup, FormArray, FormControl } from '@angular/forms';

describe('[Integration] CareerComplementaryInfoComponent', () => {
    let fixture: ComponentFixture<CareerComplementaryInfoComponent>;
    let component: CareerComplementaryInfoComponent;

    beforeEach(async(() => {
      TestBed.configureTestingModule({
        imports: [
          BrowserModule,
          MaterialComponentsModule,
          FormsModule,
          ReactiveFormsModule
        ],
        declarations: [
          CareerComplementaryInfoComponent
        ]
      }).compileComponents().then(() => {
        fixture = TestBed.createComponent(CareerComplementaryInfoComponent);
        component = fixture.componentInstance;

        component.formGroup = new FormGroup({
          'rewards': new FormArray([
            new FormGroup({
              'title': new FormControl(''),
              'name': new FormControl(''),
              'link': new FormControl(''),
              'date': new FormControl('')
            })
          ]),
          'certificates': new FormArray([
            new FormGroup({
              'title': new FormControl(''),
              'link': new FormControl('')
            })
          ]),
          'fixedLanguages': new FormArray([
            new FormGroup({
              'names': new FormControl(''),
              'languages': new FormControl(''),
              'level': new FormControl('')
            })
          ]),
          'skills': new FormArray([
            new FormGroup({
              'name': new FormControl('')
            })
          ])
        });
      });
    }));

    it('should emit addReward event on add reward button click', () => {
        const emitSpy = spyOn(component.addReward, 'emit');

        const button = fixture.nativeElement.querySelector('button.rewardButton');
        button.dispatchEvent(new Event('click'));
        fixture.detectChanges();

        expect(emitSpy).toHaveBeenCalled();
    });

    it('should fill reward correctly', () => {
      fixture.detectChanges();
      setFieldValue('div.rewardTitle input', 'Nome e colocação alcançada');
      setFieldValue('div.rewardName input', 'Instituição Responsável');
      setFieldValue('div.rewardLink input', 'Custom link');
      setFieldValue('div.rewardDate input', '2000-01-01');

      fixture.detectChanges();
      expect(component.formGroup.valid).toBeTruthy();
      const result = component.formGroup.getRawValue();
      expect(result.rewards).toBeDefined();
      expect(result.rewards.length).toBe(1);
      expect(result.rewards[0].title).toBe('Nome e colocação alcançada');
      expect(result.rewards[0].name).toBe('Instituição Responsável');
      expect(result.rewards[0].link).toBe('Custom link');
      expect(result.rewards[0].date).toEqual( new Date('2000-01-01') );
    });

    it('should emit addCertificate event on add certificate button click', () => {
      const emitSpy = spyOn(component.addCertificate, 'emit');

      const button = fixture.nativeElement.querySelector('button.certificateButton');
      button.dispatchEvent(new Event('click'));
      fixture.detectChanges();

      expect(emitSpy).toHaveBeenCalled();
    });

    it('should fill certificate correctly', () => {
      fixture.detectChanges();
      setFieldValue('div.certificateTitle input', 'Nome do certificado');
      setFieldValue('div.certificateLink input', 'Custom link');

      fixture.detectChanges();
      expect(component.formGroup.valid).toBeTruthy();
      const result = component.formGroup.getRawValue();
      expect(result.certificates).toBeDefined();
      expect(result.certificates.length).toBe(1);
      expect(result.certificates[0].title).toBe('Nome do certificado');
      expect(result.certificates[0].link).toBe('Custom link');
    });

    it('should emit addLanguage event on add language button click', () => {
        const emitSpy = spyOn(component.addLanguage, 'emit');

        const button = fixture.nativeElement.querySelector('button.languageButton');
        button.dispatchEvent(new Event('click'));
        fixture.detectChanges();

        expect(emitSpy).toHaveBeenCalled();
    });

    it('should emit addSkill event on add skill button click', () => {
      const emitSpy = spyOn(component.addSkill, 'emit');

      const button = fixture.nativeElement.querySelector('button.skillButton');
      button.dispatchEvent(new Event('click'));
      fixture.detectChanges();

      expect(emitSpy).toHaveBeenCalled();
    });

    it('should fill skill correctly', () => {
      fixture.detectChanges();
      setFieldValue('div.skillName input', 'Nome da competência');

      fixture.detectChanges();
      expect(component.formGroup.valid).toBeTruthy();
      const result = component.formGroup.getRawValue();
      expect(result.skills).toBeDefined();
      expect(result.skills.length).toBe(1);
      expect(result.skills[0].name).toBe('Nome da competência');
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
