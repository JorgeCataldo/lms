import { async, TestBed, ComponentFixture } from '@angular/core/testing';
import { BrowserModule } from '@angular/platform-browser';
import { modulesMock } from '../mocks';
import { FormsModule } from '@angular/forms';
import { MatCheckboxModule } from '@angular/material';
import { SuggestionModuleSelectComponent } from '../../manage-suggestion/module-select/module-select.component';

const dbModule = modulesMock[0];

describe('[Integration] SuggestionModuleSelectComponent', () => {

  let fixture: ComponentFixture<SuggestionModuleSelectComponent>;
  let component: SuggestionModuleSelectComponent;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      imports: [
        BrowserModule,
        FormsModule,
        MatCheckboxModule
      ],
      declarations: [
        SuggestionModuleSelectComponent
      ]
    }).compileComponents().then(() => {
      fixture = TestBed.createComponent(SuggestionModuleSelectComponent);
      component = fixture.componentInstance;
      component.module = dbModule;
    });
  }));

  it('should display the module image', async () => {
    fixture.detectChanges();
    const imgElement = fixture.nativeElement.querySelector('img.main-img');
    expect(imgElement.src).toContain(dbModule.imageUrl);
  });

  it('should display the module title', async () => {
    fixture.detectChanges();
    const titleElement = fixture.nativeElement.querySelector('div.preview h3');
    expect(titleElement.textContent).toContain(dbModule.title);
  });
});
