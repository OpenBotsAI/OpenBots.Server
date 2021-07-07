import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AddBusinessEventComponent } from './add-business-event.component';

describe('AddBusinessEventComponent', () => {
  let component: AddBusinessEventComponent;
  let fixture: ComponentFixture<AddBusinessEventComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AddBusinessEventComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AddBusinessEventComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
