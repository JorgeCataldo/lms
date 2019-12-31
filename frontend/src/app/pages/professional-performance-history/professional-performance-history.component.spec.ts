import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ProfessionalPerformanceHistoryComponent } from './professional-performance-history.component';

describe('ProfessionalPerformanceHistoryComponent', () => {
  let component: ProfessionalPerformanceHistoryComponent;
  let fixture: ComponentFixture<ProfessionalPerformanceHistoryComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ProfessionalPerformanceHistoryComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ProfessionalPerformanceHistoryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
