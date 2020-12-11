import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AllIntegrationLogsComponent } from './all-integration-logs.component';

describe('AllIntegrationLogsComponent', () => {
  let component: AllIntegrationLogsComponent;
  let fixture: ComponentFixture<AllIntegrationLogsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AllIntegrationLogsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AllIntegrationLogsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
