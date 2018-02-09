import { Component, Input, ViewChild, ElementRef, OnInit, Inject } from '@angular/core';
import { IBlobImage } from '../blobImage';

@Component({
  selector: 'imgapp-blob',
  templateUrl: './blob.component.html',
  styleUrls: ['./blob.component.css']
})
export class BlobComponent implements OnInit {

  @Input('blobImage')private blobImage: IBlobImage;
  @ViewChild('img')private img: ElementRef;

  caption = 'Not Loaded';

  constructor(@Inject('BaseUrl')private baseUrl: string) { }

  ngOnInit() {
    let image = this.img.nativeElement as HTMLImageElement;
    if (this.blobImage) {
      image.src = `${this.baseUrl}ShowImage?href=${this.blobImage.Url}`;
      image.alt = this.caption = this.blobImage.Caption;
    }
  }

}
