import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AddCredentialsComponent } from './add-credentials.component';

describe('AddCredentialsComponent', () => {
  let component: AddCredentialsComponent;
  let fixture: ComponentFixture<AddCredentialsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AddCredentialsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AddCredentialsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
