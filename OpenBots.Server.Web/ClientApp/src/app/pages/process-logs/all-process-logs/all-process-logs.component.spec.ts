import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AllProcessLogsComponent } from './all-process-logs.component';

describe('AllProcessLogsComponent', () => {
  let component: AllProcessLogsComponent;
  let fixture: ComponentFixture<AllProcessLogsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AllProcessLogsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AllProcessLogsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
