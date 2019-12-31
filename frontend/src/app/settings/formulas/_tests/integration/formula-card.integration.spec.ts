import { async, TestBed, ComponentFixture } from '@angular/core/testing';
import { BrowserModule } from '@angular/platform-browser';
import { FormulaCardComponent } from '../../formula-card/formula-card.component';
import { formulas } from '../mocks';

describe('[Integration] FormulaCardComponent', () => {

  let fixture: ComponentFixture<FormulaCardComponent>;
  let component: FormulaCardComponent;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      imports: [
        BrowserModule
      ],
      declarations: [
        FormulaCardComponent
      ]
    }).compileComponents().then(() => {
      fixture = TestBed.createComponent(FormulaCardComponent);
      component = fixture.componentInstance;
      component.formula = formulas[0];
    });
  }));

  it('should display the Formula title', async () => {
    fixture.detectChanges();
    const titleElement = fixture.nativeElement.querySelector('div.preview h3');
    expect(titleElement.textContent).toContain(formulas[0].title);
  });

  it('should display the Formula formatted creation date', async () => {
    fixture.detectChanges();
    const titleElement = fixture.nativeElement.querySelector('div.preview p.content');
    expect(titleElement.textContent).toContain('20/03/2019');
  });

  it('should emit Edit Formula event', async () => {
    const editSpy = spyOn(component.editFormula, 'emit');

    const editImage = fixture.nativeElement.querySelector('div.edit img:first-child');
    editImage.dispatchEvent(new Event('click'));

    fixture.detectChanges();
    expect(editSpy).toHaveBeenCalled();
  });

  it('should emit Delete Formula event', async () => {
    const deleteSpy = spyOn(component.deleteFormula, 'emit');

    const deleteImage = fixture.nativeElement.querySelector('div.edit img:last-child');
    deleteImage.dispatchEvent(new Event('click'));

    fixture.detectChanges();
    expect(deleteSpy).toHaveBeenCalled();
  });

});
