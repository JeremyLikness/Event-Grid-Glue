import { Inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { IBlobImage } from './blobImage';

@Injectable()
export class GetImagesService {

  private api: string;

  constructor(@Inject('BaseUrl')baseUrl: string, private http: HttpClient) {
    this.api = `${baseUrl}ListImages`;
   }

  getImages(): Observable<IBlobImage[]> {
    return this.http.get<IBlobImage[]>(this.api);
  }
}
