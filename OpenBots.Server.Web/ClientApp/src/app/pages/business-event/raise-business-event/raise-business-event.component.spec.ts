import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { RaiseBusinessEventComponent } from './raise-business-event.component';

describe('RaiseBusinessEventComponent', () => {
  let component: RaiseBusinessEventComponent;
  let fixture: ComponentFixture<RaiseBusinessEventComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ RaiseBusinessEventComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(RaiseBusinessEventComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
