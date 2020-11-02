import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { RefreshHangfireComponent } from './refresh-hangfire.component';

describe('RefreshHangfireComponent', () => {
  let component: RefreshHangfireComponent;
  let fixture: ComponentFixture<RefreshHangfireComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ RefreshHangfireComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(RefreshHangfireComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
